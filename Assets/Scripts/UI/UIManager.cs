using System.Collections.Generic;
using UnityEngine;

namespace GolfGame.UI
{
    /// <summary>
    /// Manages UI screen stack. Handles showing/hiding screens with optional history.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private List<UIScreen> allScreens;

        private Stack<UIScreen> screenStack = new();
        private UIScreen currentScreen;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Hide all screens at start
            foreach (var screen in allScreens)
            {
                if (screen != null)
                    screen.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            // Auto-show the first screen if one exists
            if (allScreens != null && allScreens.Count > 0 && allScreens[0] != null)
            {
                ShowScreen(allScreens[0], addToHistory: false);
            }
        }

        public void ShowScreen<T>() where T : UIScreen
        {
            foreach (var screen in allScreens)
            {
                if (screen is T)
                {
                    ShowScreen(screen);
                    return;
                }
            }
            Debug.LogWarning($"UIManager: Screen of type {typeof(T).Name} not found");
        }

        public void ShowScreen(UIScreen screen, bool addToHistory = true)
        {
            if (currentScreen != null)
            {
                if (addToHistory)
                    screenStack.Push(currentScreen);
                currentScreen.Hide();
            }

            currentScreen = screen;
            currentScreen.Show();
        }

        public void GoBack()
        {
            if (screenStack.Count == 0) return;

            currentScreen?.Hide();
            currentScreen = screenStack.Pop();
            currentScreen.Show();
        }

        public void HideAll()
        {
            currentScreen?.Hide();
            currentScreen = null;
            screenStack.Clear();
        }

        public T GetScreen<T>() where T : UIScreen
        {
            foreach (var screen in allScreens)
            {
                if (screen is T typed)
                    return typed;
            }
            return null;
        }
    }

    /// <summary>
    /// Base class for all UI screens.
    /// </summary>
    public abstract class UIScreen : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public virtual void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}
