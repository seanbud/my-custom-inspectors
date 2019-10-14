using UnityEngine;
using UnityEditor;
using System.Linq;

// @author : Sean Cory
#if UNITY_EDITOR
public class UtilityWindow : EditorWindow {

    Texture2D playerIcon, portalIcon, bossIcon, godmodeIcon;
    private delegate void DrawComponentFuncPtr();

    #region startup
        [MenuItem("StarQuest/UtilityWindow")]
        private static void ShowWindow() {
            var window = GetWindow<UtilityWindow>();
            window.titleContent = new GUIContent("Utility Window");
            window.FindImages();
            window.Show();
        }

        private void FindImages() {
            playerIcon = (Texture2D)Resources.Load("ShipIcon");
            portalIcon = (Texture2D)Resources.Load("PortalIcon");
            bossIcon = (Texture2D)Resources.Load("BossIcon");
            godmodeIcon = (Texture2D)Resources.Load("GodmodeIcon");
        }

        GUIStyle buttonLabelStyle, thinButtonStyle, timeLabelStyle;
        private void LoadStyles() {
            // Labels
            buttonLabelStyle = new GUIStyle(EditorStyles.label);
            buttonLabelStyle.padding.left += 5;

            timeLabelStyle = new GUIStyle(EditorStyles.label);
            timeLabelStyle.fontStyle = FontStyle.Bold;
            timeLabelStyle.padding.left = EditorStyles.foldoutHeader.padding.left;
        }
    #endregion 

    // Draw GUI components (buttons, textboxes, etc.)
    private void OnGUI() {
        if(playerIcon == null) FindImages();
        if(buttonLabelStyle==null) LoadStyles();
        
        using(new GUILayout.HorizontalScope(GUILayout.MaxWidth(150))) {
            GUILayout.Space(25);
            PlaceFrame("Select", 
            ()=> {
                PlaceButton(SelectPlayerButton, true);
            }); GUILayout.Space(15);


            PlaceFrame("Go To",
            ()=> {
                PlaceButton(GoInPortalButton, Application.isPlaying);
                PlaceButton(GoToBossButton, Application.isPlaying);
            }); GUILayout.Space(15);

            PlaceFrame("Gameplay", 
            ()=> {
                PlaceButton(DrawGodModeButton, Application.isPlaying);
            });
        }
    }

    #region Buttons
        private void SelectPlayerButton() {
            Btn("ship", playerIcon, GUILayout.Width(40), GUILayout.Height(40), ()=> 
            {
                // Selection.activeGameObject = FindEvenIfInactive("ship");
                Selection.activeGameObject = GameObject.Find("Manager").GetComponent<StateManager>().ships[0];
                Ping();

                if (DoubleClick()) {
                    SceneView.FrameLastActiveSceneViewWithLock();
                }
            });
        }
        
        private void GoInPortalButton() {
            buttonLabelStyle.padding.left -= 2; // Slide label left 2px 
            Btn("Portal", portalIcon, GUILayout.Width(40), GUILayout.Height(40), ()=> 
            {
                // Move through portal
                GameObject.Find("ship").transform.position = new Vector3(0,15,0);
            });
            buttonLabelStyle.padding.left += 2; // Reset btn label style
        }

        private void GoToBossButton() {
            Btn("Boss", bossIcon, GUILayout.Width(42), GUILayout.Height(40), ()=> 
            {
                // Go to boss
                Selection.activeGameObject.transform.position = ActiveGameObjectTracker.Instance.containerTracker.ACTIVEBOSSES[0].transform.position + Vector3.down * 25;
            });
        }

        private void DrawGodModeButton() {
            using(new GUILayout.VerticalScope()){
                GUILayout.Space(5); // Top Padding

                // Get current state
                bool godmode = false;
                try {
                    godmode = StateManager.instance.godMode;
                    GUI.backgroundColor = godmode? Color.yellow : GUI.backgroundColor;
                } catch {}
                
                // Godmode btn
                if(GUILayout.Button(godmodeIcon, GUILayout.Width(42), GUILayout.Height(40))) {
                    // Toggle On/Off
                    StateManager.instance.godMode = !StateManager.instance.godMode;
                }

                // offset label
                buttonLabelStyle.padding.left += 3;
                GUILayout.Label("God", buttonLabelStyle);
                buttonLabelStyle.padding.left -= 3;
            }
        }
    #endregion

    #region ComponenetHelpers
        private void PlaceFrame(string frameLabel, DrawComponentFuncPtr fnPtr) {
            using(new GUILayout.VerticalScope()) {
                
                GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
                style.fontStyle = FontStyle.Bold;
                GUILayout.Label(frameLabel, style);

                using(new GUILayout.HorizontalScope("helpbox")) {
                    GUILayout.Space(5); // Left Padding
                    fnPtr();
                    GUILayout.Space(12); // Right Padding
                }
            }
        }

        private void PlaceButton(DrawComponentFuncPtr funcPtr, bool enabled) {
            bool previous = GUI.enabled;
            GUI.enabled = enabled;

            funcPtr();

            GUI.enabled = previous;
        }

        private void Btn(string label, Texture2D img, GUILayoutOption w, GUILayoutOption h,  DrawComponentFuncPtr DrawButton_funcPtr) { 
            using(new GUILayout.VerticalScope()) {
                GUILayout.Space(5); // Left Padding
                if(GUILayout.Button(img, w, h)) {
                    DrawButton_funcPtr();
                }

                // Draw Label
                GUILayout.Label(label, buttonLabelStyle);
            }
        }
    #endregion

    #region WindowFunctionality
        // Double Click
        double clicked = 0, clicktime = 0, clickdelay = 1.0f;
        bool DoubleClick() {
            if (Event.current.button == 0) {
                clicked++;
                if (clicked == 1) { 
                    clicktime = EditorApplication.timeSinceStartup;
                }
            }
            if (clicked > 1 && (EditorApplication.timeSinceStartup - clicktime) < clickdelay) {
                clicked = 0;
                clicktime = 0;
                return true;
            } else if (clicked > 2 || EditorApplication.timeSinceStartup - clicktime > 1) { 
                clicked = 0;
            }
            return false;
        }

        private void Ping() {
            if (!Selection.activeObject)
            {
                Debug.Log("Select an object to ping");
                return;
            }

            EditorGUIUtility.PingObject(Selection.activeObject);
        }
    #endregion
}
#endif