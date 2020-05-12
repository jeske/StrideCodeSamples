using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;

using Stride.Extensions;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Graphics;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace RuntimeLoadWavefrontOBJ
{
    public class LoadWavefrontOBJ : SyncScript
    {
        // Declared public member fields and properties will show in the game studio

        public override void Start()
        {
            // Initialization of the script.
            Game.Window.AllowUserResizing = true;

            LoadAssetTest(new Vector3(0f,2f,0f));
        }

        public override void Update()
        {
            // Do stuff every new frame
        }


        
        private Texture textureObjectForWfTex(string textureFilename) {           
            var diffTexStream = System.IO.File.Open(textureFilename,System.IO.FileMode.Open,System.IO.FileAccess.Read);
            var textureObject = Texture.Load(GraphicsDevice,diffTexStream);
         
            return textureObject;
        }

        public void LoadAssetTest(Vector3 position) {
        
            // Create a new entity and add it to the scene.
            var entity = new Entity();
            
            var rootScene = SceneSystem.SceneInstance.RootScene;
            entity.Transform.RotationEulerXYZ = new Vector3(0,20,0);
            entity.Transform.Scale = new Vector3(0.2f,0.2f,0.2f);            
            entity.Transform.Position = position;            
            
            
            
            // Create a new model from code
            // https://doc.xenko.com/latest/en/manual/scripts/create-a-model-from-code.html
            
            // Create a model and assign it to the model component.
            var model = new Stride.Rendering.Model();
            entity.GetOrCreate<ModelComponent>().Model = model;  

            // Add one or more meshes using geometric primitives (eg spheres or cubes).
            //var meshDraw = Stride.Graphics.GeometricPrimitives.GeometricPrimitive.Sphere.New(GraphicsDevice).ToMeshDraw();
            //var mesh = new Stride.Rendering.Mesh { Draw = meshDraw }; 
            //model.Meshes.Add(mesh);            

            // create the Mesh
            // https://github.com/stride3d/stride/blob/master/sources/editor/Stride.Assets.Presentation/AssetEditors/Gizmos/LightSpotGizmo.cs#L168

            var CWD = System.IO.Directory.GetCurrentDirectory();
            var assetBase = System.IO.Path.GetFullPath(System.IO.Path.Combine(CWD,@"..\..\..\DynLoadAssets\drone2\"));
            var assetPath = (System.IO.Path.Combine(assetBase,"Drone2.obj"));

            if (!System.IO.File.Exists(assetPath)) {     
                // not sure why, but DebugText.Print isn't working at the time of testing this...
                DebugText.Print("Cannot find wavefront OBJ file at : " + assetPath, new Int2(50,50));
                return;
            }

            // load the wavefront OBJ file...
            var wfData = new SimpleScene.Util3d.WavefrontObjLoader(assetPath);
                        
            // TODO: iterate over materials / multiple materials on the same mesh...
            {                
                VertexPositionNormalTexture[] vertices;                                
                UInt32[] triIndices;
                Wavefront_VertexSoup_Stride3d.generateDrawIndexBuffer(wfData,wfData.materials[0],out triIndices, out vertices);
                                        
                // convert into a graphics VB / IB pair
                var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(GraphicsDevice, vertices, GraphicsResourceUsage.Dynamic);            
                var indexBuffer = Stride.Graphics.Buffer.Index.New(GraphicsDevice, triIndices);

                // add them to the drawing 
                var meshDraw = new Stride.Rendering.MeshDraw { 
                            /* Vertex buffer and index buffer setup */ 
                            PrimitiveType = Stride.Graphics.PrimitiveType.TriangleList,
                            DrawCount = triIndices.Length,
                            VertexBuffers = new[] { new VertexBufferBinding(vertexBuffer, VertexPositionNormalTexture.Layout, vertexBuffer.ElementCount) },               
                            IndexBuffer = new IndexBufferBinding(indexBuffer, true, triIndices.Length),                            
                        };

                // GenerateTangentBinormal() won't work on a GPU buffer. It has to be run when the data is attached to a
                //    fake CPU buffer. (see build pipeline code)
                // meshDraw.GenerateTangentBinormal();

                var customMesh = new Stride.Rendering.Mesh { Draw = meshDraw };    
                

                // set the material index for this mesh
                // customMesh.MaterialIndex = 0;

                // add the mesh to the model                
                model.Meshes.Add(customMesh);
            }


            // load a texture from a file            
            // var diffuseTextureFilename = System.IO.Path.Combine(assetBase,wfData.materials[0].mtl.diffuseTextureResourceName);
            // var diffTexStream = System.IO.File.Open(diffuseTextureFilename,System.IO.FileMode.Open,System.IO.FileAccess.Read);
            // var diffuseTexture = Texture.Load(GraphicsDevice,diffTexStream);

            // TODO: handle null textures 

            var diffuseTexture = textureObjectForWfTex(System.IO.Path.Combine(assetBase,wfData.materials[0].mtl.diffuseTextureResourceName));
            var specularTexture = textureObjectForWfTex(System.IO.Path.Combine(assetBase,wfData.materials[0].mtl.specularTextureResourceName));            
            var emissiveTexture = textureObjectForWfTex(System.IO.Path.Combine(assetBase,wfData.materials[0].mtl.ambientTextureResourceName));
            
            // note: bump/normal mapping won't won't work until Bitangents are calculated
            // var bumpTexture = textureObjectForWfTex(System.IO.Path.Combine(assetBase,wfData.materials[0].mtl.bumpTextureResourceName));                       
                        

            var cc = new ComputeColor();
                        
            #if (true) 
            {  // load textures
            
                var materialDescription = new Stride.Rendering.Materials.MaterialDescriptor
                {
                    Attributes =
                    {                   
                        DiffuseModel = new MaterialDiffuseLambertModelFeature(),                        
                        Diffuse = new MaterialDiffuseMapFeature(new ComputeTextureColor(diffuseTexture)),
                        
                        SpecularModel = new MaterialSpecularMicrofacetModelFeature{} ,
                        Specular = new MaterialSpecularMapFeature{ SpecularMap = new ComputeTextureColor(specularTexture)},
                        MicroSurface = new MaterialGlossinessMapFeature{ GlossinessMap = new ComputeFloat(0.7f) },

                        Emissive = new MaterialEmissiveMapFeature(new ComputeTextureColor(emissiveTexture)),


                        // note: normal maps won't work until bitangents are calculated
                        // https://gist.github.com/johang88/3f175b045c8e8b55fb815cc19e6128ba
                        // see TNBExtensions.GenerateTangentBinormal(this MeshDraw meshData)                        
                        //Surface = new MaterialNormalMapFeature { 
                        //        NormalMap = new ComputeTextureColor(bumpTexture),
                        //        IsXYNormal = true,
                        //        ScaleAndBias = true,                                
                        //        },
                       
                        // this is for a solid color rendering...
                        // Diffuse = new MaterialDiffuseMapFeature(new ComputeColor { Key = MaterialKeys.DiffuseValue }),

                    }
                };
                var material = Material.New(GraphicsDevice, materialDescription);            
                material.Passes[0].Parameters.Set(MaterialKeys.EmissiveIntensity,5.0f);                
                model.Materials.Add(material);                
           } 
            #else
                // this is for solid color rendering...
                var material = Material.New(GraphicsDevice, new MaterialDescriptor());            
                material.Passes[0].Parameters.Set(MaterialKeys.DiffuseValue, Color.Red);     
                model.Materials.Add(material);
            #endif
           
            

           SceneSystem.SceneInstance.RootScene.Entities.Add(entity);

        }
               


    }
}
