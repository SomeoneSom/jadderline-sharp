namespace Jadderline {
    public class Jadderline {
        const float DeltaTime = 0.0166667f;
        const float FrictionNorm = (float)((double)1000f * (double)0.65f);
        const float FrictionOverMax = (float)((double)400f * (double)0.65f);
        const float FrictionNormHold = FrictionNorm * 0.5f;
        const float FrictionOverMaxHold = FrictionOverMax * 0.5f;

        // Most inputs are self explanatory, though for direction, false is left and true is right
        // Additionally, jelly2 is the one on cooldown (jelly1 is the one about to be grabbed, and as such, doesnt need to be inputted)
        // The additional inputs may or may not need commas, not 100% sure
        // This doesnt go back a frame if an impossible frame is reached, but ive never seen it ever go back in jadderline so its not too much of a priority currently
        public static string Run(double playerPos, float playerSpeed, float jelly2Pos, int ladders, bool direction, bool moveOnly, string additionalInputs) {
            if (ladders < 2) { // Because we calculate the jelly ladders in 2 regrab windows
                throw new ArgumentException("Must calculate at least 2 ladders");
            }
            float jelly1Pos = (float)playerPos; // Since this is the one we are about to grab
            // Get all 262144 candidates for inputs
            List<(bool[], bool[])> potential = new();
            for (int j = 0; j < 512; j++) {
                for (int k = 0; k < 512; k++) {
                    potential.Add((ToBits(j), ToBits(k)));
                }
            }
            List<float> results = new(new float[262144]);
            List<bool[]> inputs = new();
            for (int i = 0; i < ladders; i += 2) {
                // Remove last entry if needed for parity
                if (ladders - i == 1) {
                    inputs.RemoveAt(inputs.Count - 1);
                } else if (inputs.Count != 0) { // Only run this now to account for the above case
                    (playerPos, playerSpeed, jelly1Pos) = MoveVars(inputs[inputs.Count - 1], playerPos, playerSpeed, jelly1Pos, direction);
                    jelly2Pos = float.Round(jelly1Pos) + jelly2Pos - float.Truncate(jelly2Pos);
                    jelly1Pos = (float)playerPos;
                }
                Parallel.For(0, 262144, j => {
                    results[j] = Eval(potential[j], playerPos, playerSpeed, jelly1Pos, jelly2Pos, direction);
                });
                int max;
                if (direction) {
                    max = results.IndexOf(results.Max());
                } else {
                    max = results.IndexOf(results.Min());
                }
                if (results[max] == float.NegativeInfinity && results[max] == float.PositiveInfinity) {
                    throw new ArgumentException("Malformed input or impossible jelly ladder"); // Is this actually the right exception to use? No clue
                }
                inputs.Add(potential[max].Item1);
                inputs.Add(potential[max].Item2);
                (playerPos, playerSpeed, jelly1Pos) = MoveVars(potential[max].Item1, playerPos, playerSpeed, jelly1Pos, direction); // Save the result of the chosen input
                jelly2Pos = float.Round(jelly1Pos); // Make jelly1 the new jelly2
                jelly1Pos = (float)playerPos;
            }
            return Format(inputs, moveOnly, direction, additionalInputs);
        }

        // Gets the distance jelly1 has moved while ensuring the player can still grab jelly2
        // Yes this code does kind of suck but it works
        private static float Eval((bool[], bool[]) inputs, double playerPos, float playerSpeed, float jelly1Pos, float jelly2Pos, bool direction) {
            (double playerPosNew, float playerSpeedNew, float jelly1PosNew) = MoveVars(inputs.Item1, playerPos, playerSpeed, jelly1Pos, direction);
            bool wentOver; // Went past jelly
            float jelly2PosNew = float.Round(jelly1PosNew); // New jelly2 position
            jelly1PosNew = (float)playerPosNew;
            if (playerPosNew >= jelly2Pos + 13.5f || playerPosNew < jelly2Pos - 13.5f) {
                if (direction) {
                    return float.NegativeInfinity;
                } else {
                    return float.PositiveInfinity;
                }
            }
            (playerPosNew, _, _) = MoveVars(inputs.Item2, playerPosNew, playerSpeedNew, jelly1PosNew, direction);  
            if (playerPosNew >= jelly2PosNew + 13.5f || playerPosNew < jelly2PosNew - 13.5f) {
                if (direction) {
                    return float.NegativeInfinity;
                } else {
                    return float.PositiveInfinity;
                }
            } else {
                return (float)(playerPosNew - playerPos);
            }
        }

        // Actually calculates the inputs
        private static (double, float, float) MoveVars(bool[] inputs, double playerPos, float playerSpeed, float jelly1Pos, bool direction) {
            // Frame of movement on last frame of StPickup
            playerPos += playerSpeed * DeltaTime;
            // 8 frames of holding the jelly
            for (int i = 0; i < 8; i++) {
                (playerPos, playerSpeed) = MoveStep(inputs[i], playerPos, playerSpeed, true, direction);
            }
            // Release jelly1, which will be at the player's current positition when dropped
            jelly1Pos = (float)playerPos;
            // Frame of movement when you release the jelly
            (playerPos, playerSpeed) = MoveStep(inputs[8], playerPos, playerSpeed, false, direction);
            return (playerPos, playerSpeed, jelly1Pos);
        }

        // Calculates one frame of movement
        private static (double, float) MoveStep(bool input, double playerPos, float playerSpeed, bool holding, bool direction) {
            float frictionNorm;
            float frictionOverMax;
            float max;
            if (holding) {
                frictionNorm = FrictionNormHold;
                frictionOverMax = FrictionOverMaxHold;
                max = 108.00001f;
            } else {
                frictionNorm = FrictionNorm;
                frictionOverMax = FrictionOverMax;
                max = 90f;
            }
            float mult;
            if (direction) {
                mult = 1f;
            } else {
                mult = -1f;
            }
            if (!input) { // Holding neutral
                playerSpeed -= frictionNorm * DeltaTime * mult;
                if (playerSpeed * mult < 0f) {
                    playerSpeed = 0f;
                }
            } else if (playerSpeed * mult <= max) { // Coming up to max speed
                playerSpeed += frictionNorm * DeltaTime * mult;
                if (playerSpeed * mult > max) {
                    playerSpeed = max * mult;
                }
            } else { // Over max speed
                playerSpeed -= frictionOverMax * DeltaTime * mult;
                if (playerSpeed * mult < max) {
                    playerSpeed = max * mult;
                }
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
                dirString = ",R";
            } else {
                dirString = ",L";
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
                        result += $"{f.Item1},G{additionalInputs}{mString}{dirString}\n";
                    } else {
                        result += $"{f.Item1},G{additionalInputs}\n";
                    }
                }
                if (input[8]) {
                    result += $"1{additionalInputs}{mString}{dirString},D\n";
                } else {
                    result += $"1{additionalInputs}{mString},D\n";
                }
            }
            // Copy to clipboard (for easy insertion)
            TextCopy.ClipboardService.SetText(result);
            return result;
        }
    }
}