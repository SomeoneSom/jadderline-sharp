using Jadderline;

class Program {
    // This is mainly just for making sure this C# port works
    static void Main(string[] args) {
        Console.Write("playerPos: ");
        float playerPos = float.Parse(Console.ReadLine());
        Console.Write("playerSpeed: ");
        float playerSpeed = float.Parse(Console.ReadLine());
        Console.Write("jelly1Pos: ");
        float jelly1Pos = float.Parse(Console.ReadLine());
        Console.Write("jelly2Pos: ");
        float jelly2Pos = float.Parse(Console.ReadLine());
        Console.Write("ladders: "); 
        int ladders = int.Parse(Console.ReadLine());
        Console.Write("direction: ");
        bool direction = bool.Parse(Console.ReadLine());
        Console.Write("moveOnly: ");
        bool moveOnly = bool.Parse(Console.ReadLine());
        Console.Write("additionalInputs: ");
        string additionalInputs = Console.ReadLine() ?? "";
        Jadderline.Jadderline.Run(playerPos, playerSpeed, jelly1Pos, jelly2Pos, ladders, direction, moveOnly, additionalInputs);
    }
}
