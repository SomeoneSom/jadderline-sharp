namespace Jadderline {
    public class Jadderline {
        const float DeltaTime = 0.166667f;
        const float FrictionNorm = 650f;
        const float FrictionOverMax = 260f;
        const float FrictionNormHold = FrictionNorm / 2;
        const float FrictionOverMaxHold = FrictionOverMax / 2;

        // Most inputs are self explanatory, though for direction, false is left and true is right
        public static void Run(float playerPos, float playerSpeed, float jelly1Pos, float jelly2Pos, int ladders, bool direction, bool moveOnly, string additionalInputs) {
            Console.WriteLine("Test");
            return;
        }

        private static string Format() {
            return "";
        }
    }
}