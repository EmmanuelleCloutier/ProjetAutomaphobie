using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;
#if UIELEMENTS_MODULE_AVAILABLE
using UnityEngine.UIElements;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Samples.WorldSpaceUI
{
    /// <summary>
    /// Sample class that demonstrates how to bind to a UI Toolkit button click event.
    /// </summary>
    ///
    ///
    
    public class ButtonEventSample : MonoBehaviour
    {
        public Texture2D normalTexture;
        public Texture2D hoverTexture;
        public Texture2D pressedTexture;
        
        void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement; // GetComponentInParent<VisualElement>();
            
            
            Button PlayButton = root.Q<Button>("PlayButton");
            Button SettingsButton = root.Q<Button>("SettingsButton");
            Button QuitButton = root.Q<Button>("QuitButton");
            
            RegisterButtonTextures(PlayButton);
            RegisterButtonTextures(SettingsButton);
            RegisterButtonTextures(QuitButton);

            PlayButton.clicked += () => OpenLevel("Gym_Bastien_V2", 1);
            //SettingsButton.clicked += () => OpenLevel("SettingsMenu");
            QuitButton.clicked += () => Application.Quit();
        }

        private void RegisterButtonTextures(Button btn)
        {
            // Normal
            btn.style.backgroundImage = new StyleBackground(normalTexture);

            // Hover
            btn.RegisterCallback<PointerEnterEvent>(evt =>
            {
                btn.style.backgroundImage = new StyleBackground(hoverTexture);
            });

            btn.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                btn.style.backgroundImage = new StyleBackground(normalTexture);
            });

            // Pressed
            btn.RegisterCallback<PointerDownEvent>(evt =>
            {
                btn.style.backgroundImage = new StyleBackground(pressedTexture);
            });
            
            btn.RegisterCallback<ClickEvent>(evt =>
            {
                btn.style.backgroundImage = new StyleBackground(pressedTexture);
            });

            btn.RegisterCallback<PointerUpEvent>(evt =>
            {
                // revenir à hover si encore dessus
                btn.style.backgroundImage = new StyleBackground(normalTexture);
            });
        }

        public void OpenLevel(String LevelName, int LevelIndex)
        {
            //Application.LoadLevel(LevelName);
            SceneManager.LoadScene(LevelIndex);
        }
    }
}
