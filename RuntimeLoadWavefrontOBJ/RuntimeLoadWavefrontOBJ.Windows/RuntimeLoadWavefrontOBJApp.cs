using Stride.Engine;

namespace RuntimeLoadWavefrontOBJ.Windows
{
    class RuntimeLoadWavefrontOBJApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
