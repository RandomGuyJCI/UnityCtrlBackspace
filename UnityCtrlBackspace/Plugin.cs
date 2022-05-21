using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine.UI;

namespace UnityCtrlBackspace
{
    [BepInPlugin("com.randomguyjci.unityctrlbackspace", "Ctrl-Backspace For Unity", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(PatchInputField));
        }

        public static class PatchInputField
        {
            /*
            In order to override the key behavior for pressing ctrl-backspace and prevent the first character from being deleted prematurely, 
            we need to know when ctrl is being held in the KeyPressed() method.
            
            Based on the decompiled assembly, we know there is a flag4 variable in the function that determines if ctrl is being held,
            which means we can use a transpiler to read that value to be used in patching the Backspace method later.
            
            This essentially edits the backspace logic in KeyPressed() from
            
                case KeyCode.Backspace:
		            Backspace();
		            return EditState.Continue;
		            
		    to
		    
		        case KeyCode.Backspace:
		            isCtrlHeld = flag4;
		            Backspace();
		            return EditState.Continue;
            */
            
            private static bool isCtrlHeld = false;
            
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(InputField), "KeyPressed")]
            public static IEnumerable<CodeInstruction> KeyPressedTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                return new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(InputField), "Backspace")))
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldloc_S, 4),
                        new CodeInstruction(OpCodes.Stsfld, AccessTools.Field(typeof(PatchInputField), nameof(isCtrlHeld))))
                    .InstructionEnumeration();
            }
            
            /*
            We can now inject our own logic into the Backspace() method so that different behavior is handled for deleting text
            if ctrl-backspace is pressed. This patch is mostly copied over from the original decompiled method.
            */
            
            [HarmonyPrefix]
            [HarmonyPatch(typeof(InputField), "Backspace")]
            public static bool Prefix(InputField __instance)
            {
                if (!__instance.m_ReadOnly)
                {
                    if (__instance.hasSelection)
                    {
                        __instance.Delete();
                        __instance.UpdateTouchKeyboardFromEditChanges();
                        __instance.SendOnValueChangedAndUpdateLabel();
                    }
                    else if (__instance.caretPositionInternal > 0 && __instance.caretPositionInternal - 1 < __instance.text.Length)
                    {
                        if (isCtrlHeld)
                        {
                            BackspaceWord(ref __instance);
                        }
                        else
                        {
                            __instance.m_Text = __instance.text.Remove(__instance.caretPositionInternal - 1, 1);
                            __instance.caretSelectPositionInternal = --__instance.caretPositionInternal;
                        }
                        __instance.UpdateTouchKeyboardFromEditChanges();
                        __instance.SendOnValueChangedAndUpdateLabel();
                    }
                }
                return false;
            }
            
            /*
            The actual method that handles deleting the word. This takes advantage of a modified version of the
            FindtPrevWordBegin method that can be found from the decompiled assembly.
            */
            
            private static void BackspaceWord(ref InputField __instance)
            {
                var newCaretPosition = ModifiedFindPrevWordBegin(__instance.m_Text, __instance.caretPositionInternal);
                var moveLength = __instance.caretPositionInternal - newCaretPosition;

                __instance.m_Text = __instance.text.Remove(newCaretPosition, moveLength);
                __instance.caretPositionInternal = __instance.caretSelectPositionInternal = newCaretPosition;
            }
            
            /*
            FindtPrevWordBegin() is used by the InputField class for moving the caret word by word when pressing ctrl-left.
            Although we can directly use that method for BackspaceWord(), it's found to be inconsistent in actually finding the beginning of the previous word.
            For example, pressing ctrl-left on the end of "test string..." would move the caret to the 2nd period instead of the g.
            I therefore had to create my own modified version of that method that properly handles all edge cases.
            */
            
            private static int ModifiedFindPrevWordBegin(string text, int caretPosition)
            {
                var lastCaretPosition = caretPosition - 1;

                if (char.IsWhiteSpace(text[lastCaretPosition]))
                {
                    for (; lastCaretPosition > 0; lastCaretPosition--)
                        if (!char.IsWhiteSpace(text[lastCaretPosition])) break;
                }
                
                if (char.IsLetterOrDigit(text[lastCaretPosition]))
                {
                    for (; lastCaretPosition > 0; lastCaretPosition--)
                        if (!char.IsLetterOrDigit(text[lastCaretPosition - 1])) break;
                }
                else
                {
                    for (; lastCaretPosition > 0; lastCaretPosition--)
                        if (char.IsLetterOrDigit(text[lastCaretPosition - 1]) || char.IsWhiteSpace(text[lastCaretPosition - 1])) break;
                }
                
                return lastCaretPosition;
            }
        }
    }
}
