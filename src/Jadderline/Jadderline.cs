namespace Jadderline {
    public class Jadderline {
        const float DeltaTime = 0.166667f;
        const float FrictionNorm = 650f;
        const float FrictionOverMax = 260f;
        const float FrictionNormHold = FrictionNorm / 2;
        const float FrictionOverMaxHold = FrictionOverMax / 2;

        // Most inputs are self explanatory, though for direction, false is left and true is right
        // The additional inputs may or may not need commas, not 100% sure
        public static void Run(float playerPos, float playerSpeed, float jelly1Pos, float jelly2Pos, int ladders, bool direction, bool moveOnly, string additionalInputs) {
            List<bool[]> inputs = new List<bool[]>();
            for (int i = 0; i < ladders; i++) {

            }
            return;
        }

        // Converts an int into a bool[9]
        public static bool[] ToBits(int num) {
            bool[] bits = new bool[9];
            for (int i = 0; i < 9; i++) {
                bits[i] = (num & 1) != 0;
                num >>= 1;
            }
            return bits;
        }

        // Formats the inputs to be copy and pasted into Studio
        private static string Format(bool moveOnly, string additionalInputs) {
            return "";
        }
    }
}