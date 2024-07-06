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
            for (int i = 0; i < ladders; i++) {

            }
            return;
        }

        // Formats the inputs to be copy and pasted into Studio
        private static string Format(bool moveOnly, string additionalInputs) {
            return "";
        }
    }
}