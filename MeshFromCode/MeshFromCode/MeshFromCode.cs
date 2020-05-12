using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;

namespace MeshFromCode
{
    public class MeshFromCode : SyncScript
    {
        // Declared public member fields and properties will show in the game studio

        public override void Start()
        {
            // Initialization of the script.
            Game.Window.AllowUserResizing = true;
            MakeMeshFromCode();
        }

        public override void Update()
        {
            // Do stuff every new frame
        }

        void MakeMeshFromCode() {

            var vertices = new VertexPositionTexture[3];
            vertices[0].Position = new Vector3(0f,0f,1f);            
            vertices[1].Position = new Vector3(0f,1f,0f);
            vertices[2].Position = new Vector3(0f,1f,1f);
            var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(GraphicsDevice, vertices,
                                                                 GraphicsResourceUsage.Dynamic);
            int[] indices = { 0, 2, 1 };
            var indexBuffer = Stride.Graphics.Buffer.Index.New(GraphicsDevice, indices);

            var customMesh = new Stride.Rendering.Mesh
            { 
                Draw = new Stride.Rendering.MeshDraw
                { 
                    /* Vertex buffer and index buffer setup */ 
                    PrimitiveType = Stride.Graphics.PrimitiveType.TriangleList,
                    DrawCount = indices.Length,
                    IndexBuffer = new IndexBufferBinding(indexBuffer, true, indices.Length),
                    VertexBuffers = new[] { new VertexBufferBinding(vertexBuffer, 
                                              VertexPositionTexture.Layout, vertexBuffer.ElementCount) },
                }
            };            


            // Create a new entity 
            var entity = new Entity();
            entity.Transform.RotationEulerXYZ = new Vector3(0,20,0);
            entity.Transform.Scale = new Vector3(0.2f,0.2f,0.2f);            
            entity.Transform.Position = new Vector3(0,0,0);            

            // create a model and assign to entity
            var model = new Stride.Rendering.Model();
            entity.GetOrCreate<ModelComponent>().Model = model;  

            // add the mesh to the model
            model.Meshes.Add(customMesh);

            // this is for solid color rendering...
            var material = Material.New(GraphicsDevice, new MaterialDescriptor());            
            material.Passes[0].Parameters.Set(MaterialKeys.DiffuseValue, Color.Red);     
            model.Materials.Add(material);

            // add entity to the root scene
            SceneSystem.SceneInstance.RootScene.Entities.Add(entity);
        }
    }
}
