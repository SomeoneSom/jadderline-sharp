namespace Jadderline {
    public class Jadderline {
        const float DeltaTime = 0.0166667f;
        const float FrictionNorm = 650f;
        const float FrictionOverMax = 260f;
        const float FrictionNormHold = FrictionNorm / 2;
        const float FrictionOverMaxHold = FrictionOverMax / 2;

        // Most inputs are self explanatory, though for direction, false is left and true is right
        // Additionally, jelly2 is the one on cooldown (jelly1 is the one about to be grabbed, and as such, doesnt need to be inputted)
        // The additional inputs may or may not need commas, not 100% sure
        // This doesnt go back a frame if an impossible frame is reached, but ive never seen it ever go back in jadderline so its not too much of a priority currently
        public static string Run(float playerPos, float playerSpeed, float jelly2Pos, int ladders, bool direction, bool moveOnly, string additionalInputs) {
            if (ladders < 2) { // Because we calculate the jelly ladders in 2 regrab windows
                throw new ArgumentException("Must calculate at least 2 ladders");
            }
            if (direction) { // Jelly positions should be on one edge, and player position should be on the other
                jelly2Pos = float.Round(jelly2Pos) + 9.5f;
                playerPos -= 4f;
            } else {
                jelly2Pos = float.Round(jelly2Pos) - 10.5f;
                playerPos += 3f;
            }
            float jelly1Pos = playerPos; // Since this is the one we are about to grab
            // Get all 262144 candidates for inputs
            List<(bool[], bool[])> potential = new();
            for (int j = 0; j < 512; j++) {
                for (int k = 0; k < 512; k++) {
                    potential.Add((ToBits(j), ToBits(k)));
                }
            }
            List<float> results = new(new float[262144]);
            List<bool[]> inputs = new();
            for (int i = 0; i < ladders; i++) {
                // Remove last entry since we arent at the end yet
                if (inputs.Count != 0) {
                    inputs.RemoveAt(inputs.Count - 1);
                }
                Parallel.For(0, 262144, j => {
                    results[j] = Eval(potential[j], playerPos, playerSpeed, jelly1Pos, jelly2Pos, direction);
                });
                int max = results.IndexOf(results.Max());
                if (results[max] == float.NegativeInfinity) {
                    throw new ArgumentException("Malformed input or impossible jelly ladder"); // Is this actually the right exception to use? No clue
                }
                inputs.Add(potential[max].Item1);
                inputs.Add(potential[max].Item2);
                (playerPos, playerSpeed, jelly1Pos) = MoveVars(potential[max].Item1, playerPos, playerSpeed, jelly1Pos, direction); // Save the result of the chosen input
                if (direction) { // Make jelly1 the new jelly2
                    jelly2Pos = float.Round(jelly1Pos) + 13.5f;
                } else {
                    jelly2Pos = float.Round(jelly1Pos) - 13.5f;
                }
                jelly1Pos = playerPos;
            }
            return Format(inputs, moveOnly, direction, additionalInputs);
        }

        // Gets the distance jelly1 has moved while ensuring the player can still grab jelly2
        // Yes this code does kind of suck but it works
        private static float Eval((bool[], bool[]) inputs, float playerPos, float playerSpeed, float jelly1Pos, float jelly2Pos, bool direction) {
            (float playerPosNew, float playerSpeedNew, float jelly1PosNew) = MoveVars(inputs.Item1, playerPos, playerSpeed, jelly1Pos, direction);
            bool wentOver; // Went past jelly
            float jelly2PosNew; // New jelly2 position
            if (playerSpeedNew > 0f) { // Also make jelly1 the new jelly2
                wentOver = playerPosNew >= jelly2Pos;
                jelly2PosNew = float.Round(jelly1PosNew) + 13.5f;
            } else {
                wentOver = playerPosNew < jelly2Pos;
                jelly2PosNew = float.Round(jelly1PosNew) - 13.5f;
            }
            jelly1PosNew = playerPosNew;
            if (wentOver) {
                return float.NegativeInfinity;
            }
            (playerPosNew, _, _) = MoveVars(inputs.Item2, playerPosNew, playerSpeedNew, jelly1PosNew, direction);  
            if (playerSpeedNew > 0f) { // And again
                wentOver = playerPosNew >= jelly2PosNew;
            } else {
                wentOver = playerPosNew < jelly2PosNew;
            }
            if (wentOver) {
                return float.NegativeInfinity;
            } else {
                return float.Abs(playerPosNew - playerPos);
            }
        }

        // Actually calculates the inputs
        private static (float, float, float) MoveVars(bool[] inputs, float playerPos, float playerSpeed, float jelly1Pos, bool direction) {
            // Frame of movement on last frame of StPickup
            playerPos += playerSpeed * DeltaTime;
            // 8 frames of holding the jelly
            for (int i = 0; i < 8; i++) {
                (playerPos, playerSpeed) = MoveStep(inputs[i], playerPos, playerSpeed, true, direction);
            }
            // Release jelly1, which will be at the player's current positition when dropped
            jelly1Pos = playerPos;
            // Frame of movement when you release the jelly
            (playerPos, playerSpeed) = MoveStep(inputs[8], playerPos, playerSpeed, false, direction);
            return (playerPos, playerSpeed, jelly1Pos);
        }

        // Calculates one frame of movement
        private static (float, float) MoveStep(bool input, float playerPos, float playerSpeed, bool holding, bool direction) {
            float frictionNorm;
            float frictionOverMax;
            float max;
            if (holding) {
                frictionNorm = FrictionNormHold;
                frictionOverMax = FrictionOverMaxHold;
                max = 108f;
            } else {
                frictionNorm = FrictionNorm;
                frictionOverMax = FrictionOverMax;
                max = 90f;
            }
            playerSpeed = float.Abs(playerSpeed);
            if (!input) { // Holding neutral
                playerSpeed -= frictionNorm * DeltaTime;
                if (playerSpeed < 0f) {
                    playerSpeed = 0f;
                }
            } else if (playerSpeed <= max) { // Coming up to max speed
                playerSpeed += frictionNorm * DeltaTime;
                if (playerSpeed > max) {
                    playerSpeed = max;
                }
            } else { // Over max speed
                playerSpeed -= frictionOverMax * DeltaTime;
                if (playerSpeed < max) {
                    playerSpeed = max;
                }
            }
            if (!direction) { // Flip speed back if moving left
                playerSpeed *= -1;
            }
            playerPos += playerSpeed * DeltaTime;
            return (playerPos, playerSpeed);
        }

        // Converts an int into a bool[9]
        private static bool[] ToBits(int num) {
            bool[] bits = new bool[9];
            for (int i = 0; i < 9; i++) {
                bits[i] = (num & 1) != 0;
                num >>= 1;
            }
            return bits;
        }

        // Formats the inputs to be copy and pasted into Studio
        private static string Format(List<bool[]> inputs, bool moveOnly, bool direction, string additionalInputs) {
            string result = "";
            string dirString;
            if (direction) {
                dirString = "R";
            } else {
                dirString = "L";
            }
            string mString;
            if (moveOnly) {
                mString = "M";
            } else {
                mString = "";
            }
            foreach (var input in inputs) {
                List<(int, bool)> formatted = new() {
                    (13, false)
                };
                for (int i = 0; i < 8; i++) {
                    int last = formatted.Count - 1;
                    if (formatted[last].Item2 == input[i]) {
                        formatted[last] = (formatted[last].Item1 + 1, input[i]);
                    } else {
                        formatted.Add((1, input[i]));
                    }
                }
                foreach (var f in formatted) {
                    if (f.Item2) {
                        result += $"{f.Item1}G{additionalInputs}{mString}{dirString}\n";
                    } else {
                        result += $"{f.Item1}G{additionalInputs}\n";
                    }
                }
                if (input[8]) {
                    result += $"1{additionalInputs}{mString}{dirString}D\n";
                } else {
                    result += $"1{additionalInputs}{mString}D\n";
                }
            }
            // Copy to clipboard (for easy insertion)
            TextCopy.ClipboardService.SetText(result);
            return result;
        }
    }
}