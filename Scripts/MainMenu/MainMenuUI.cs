using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class MainMenuUI : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;
    public TextMeshProUGUI loadingText;
    public RawImage blackScreen;
    public RectTransform credits;
    public GameObject crate;
    public Image fade;
    //camera
    public Camera cam;
    private InputSystem_Actions inputActions;
    private Vector2 mousePos;
    private Vector2 smoothedMouse;
    private Vector2 smoothedMouseVelocity;
    private Vector2 screenCenter;
    public float smoothSpeed;
    public float strength;
    private float spawnTimer;
    private float creditsPosition;
    private float fadeAmount;
    void OnEnable()
    {
        inputActions.Player.Enable();
    }
    void OnDisable()
    {
        inputActions.Player.Disable();
    }
    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            playButton.gameObject.SetActive(true);
            quitButton.gameObject.SetActive(true);
            loadingText.gameObject.SetActive(false);
            blackScreen.gameObject.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;

        cam = FindAnyObjectByType<Camera>();
        
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        Time.timeScale = 1;
        spawnTimer = 2f;
        creditsPosition = -800;
        fadeAmount = 1;
    }
    void Update()
    {
        if(SceneManager.GetActiveScene().name == "Purgatory")
        {
            spawnTimer -= Time.deltaTime;
            if(spawnTimer <= 0)
            {
                Instantiate(crate, new Vector3(-5, 25, 4), Quaternion.identity);
                spawnTimer = 2f;
            }
            creditsPosition += Time.unscaledDeltaTime * 20;
            credits.anchoredPosition = new Vector2(-690, creditsPosition);

            fadeAmount -= Time.deltaTime / 5;
            fadeAmount = Mathf.Clamp(fadeAmount, 0, 1);
            fade.color = new Color(
                fade.color.r, fade.color.g, fade.color.b,
                fadeAmount
            );
        }

        mousePos = Mouse.current.position.ReadValue();
        smoothedMouse = Vector2.SmoothDamp(smoothedMouse, 
        (mousePos - screenCenter) * strength, 
        ref smoothedMouseVelocity, smoothSpeed);

        smoothedMouse = Vector2.ClampMagnitude(smoothedMouse, 25);
        
        cam.transform.rotation = Quaternion.Euler(10 + -smoothedMouse.y, -45 + smoothedMouse.x, 0);
    }
    public void Play()
    {
        playButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
        loadingText.gameObject.SetActive(true);
        blackScreen.gameObject.SetActive(true);

        SceneManager.LoadScene("Field");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
