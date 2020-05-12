using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Graphics;
using Stride.UI.Panels;
using Stride.UI.Controls;
using Stride.UI;
using System.Diagnostics;


// HOWTO USE THIS CODE IN YOUR OWN SCRIPT:
//
// 1. you have to make a "UI Entity" (any Entity with a "UI" Component)
// 2. add this script as an asset to your project
// 3. add a script component to the above entity, pointing at this script
// 4. create a font assets 
// 5. click on the UI entity in your scene, look for the script component, and assign "myFont" to your font asset

namespace DynamicScrollingButtonList
{
    public class DynamicScrollingButtonList : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
              
        public SpriteFont myFont;   // see #4/5 above for setup

        public override void Start()
        {
            // Initialization of the script.
            Game.Window.AllowUserResizing = true;

            //////// I'd like to search for a dynamic runtime font in code, but I'm not sure how...
            // var fontSystem = Services.GetService<FontSystem>();
            // var myFont = fontSystem.NewDynamic(10,"Orkney Regular",FontStyle.Regular);

            // our dynamic UI create code...
            CreateUI();             
        }

        public override void Update()
        {
            // Do stuff every new frame
        }




        
        public void CreateUI() {

            // https://github.com/stride3d/stride/blob/master/samples/Tutorials/CSharpBeginner/CSharpBeginner/CSharpBeginner.Game/Code/TutorialUI.cs#L34


            // 2. create our grid of buttons...
            var numButtons = 50;
            
            var grid = new UniformGrid{
                Name = "grid 1",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                BackgroundColor = Color.Yellow,
                Columns = 2, Rows = numButtons,
            };
            var scrollV = new ScrollViewer{
                Name = "scroll 1",
                ScrollMode = ScrollingMode.Vertical,
                BackgroundColor = Color.Purple,
                HorizontalAlignment = HorizontalAlignment.Stretch,                
                VerticalAlignment = VerticalAlignment.Stretch,                
                ScrollBarThickness = 50,                     
            };
            

            var mainCanvas = new Canvas{                           
                BackgroundColor = new Color(1.0f,0f,0f,0.5f),
            };
            


            // https://github.com/stride3d/stride/blob/273dfddd462fd3746569f833e1493700c070b14d/sources/engine/Stride.UI.Tests/Regression/CanvasGridTest.cs
            for (int i=0; i< numButtons; i++) 
            { 
                var cur_i = i;
                var startButton = new Button {
                    Content = new TextBlock {
                        Text = "Create Object #" + i,
                        Font = myFont, TextColor = Color.Black, 
                        HorizontalAlignment = HorizontalAlignment.Center, 
                        VerticalAlignment = VerticalAlignment.Center,
                        BackgroundColor = Color.LightBlue,                  
                        },                
                    Padding = new Thickness(77, 30, 25, 30),
                    ClickMode = ClickMode.Press,
                    BackgroundColor = Color.Green,                    
                    MinimumWidth = 250f,                
                };
                var objPos = new Vector3(0f,4f,1f * i);
                startButton.Click += (object sender, Stride.UI.Events.RoutedEventArgs e) => 
                    { 
                        // do something to show we clicked a button
                        // like set the text of some other UI control                        
                        DebugText.Print("Button Clicked #" + cur_i, new Int2(50,50));                       
                    };

                startButton.DependencyProperties.Set(GridBase.RowPropertyKey,i/2);
                startButton.DependencyProperties.Set(GridBase.ColumnPropertyKey,i%2);

            
                grid.Children.Add(startButton);
            }
            
            scrollV.Content = grid;            
            mainCanvas.Children.Add(scrollV);
            
            var mainMenuRoot = new ModalElement{
                Width = 500,
                Height = 500,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                DefaultHeight = 200,
                OverlayColor = new Color(0.5f,0f,0f,0.5f), // clear                
                Content = mainCanvas,
                };            

            Entity.Get<UIComponent>().Page = new UIPage { RootElement = mainMenuRoot };                        
        }
     

        private Texture textureObjectForWfTex(string textureFilename) {           
            var diffTexStream = System.IO.File.Open(textureFilename,System.IO.FileMode.Open,System.IO.FileAccess.Read);
            var textureObject = Texture.Load(GraphicsDevice,diffTexStream);
         
            return textureObject;
        }


    }
}
