using Stride.Engine;

namespace MeshFromCode.Windows
{
    class MeshFromCodeApp
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
