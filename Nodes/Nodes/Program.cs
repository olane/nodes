using System;

namespace Nodes
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (NodesGame game = new NodesGame())
            {
                game.Run();
            }
        }
    }
#endif
}

