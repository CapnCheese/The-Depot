using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.LookDev;
using System.Collections;
public class UI : MonoBehaviour
{
    //References
    public PlayerSettings playerSettings;
    public CameraMovement cam;
    public PlayerMovement player;
    public PlayerInput playerInput;
    public ItemSwitcher itemSwitcher;
    public Timer timer;
    public CollectionZone collectionZone;
    public Volume volume;
    public Material fogMaterial;
    public ProceduralGeneration proceduralGeneration;
    public Truck truck;
    public Nextbot nextbot;
    public GameObject portal;
    //UI Objects
    public TextMeshProUGUI displaySpeed;
    public TextMeshProUGUI displayFPS;
    public TextMeshProUGUI sensitivityText;
    public TextMeshProUGUI grassDistanceText;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI deathText;
    public Image slot1;
    public Image slot2;
    public Image slot3;
    public RectTransform bar1;
    public RectTransform bar2;
    public RawImage blackScreen;
    //Interactable
    public Button mainMenuButton;
    public Slider sensitivitySlider;
    public Slider grassDistanceSlider;
    //Other
    public bool pauseMenu;
    public bool died;
    private float speed;
    private Scene currentScene;
    private float deltaTime;
    private float frameCount;
    private float FPS;
    public float health;
    private float loadingTimer;
    private float softlockTimer;
    private bool softlock;
    private Bloom bloom;
    public bool loading;
    public Material sky;
    public Material darkSky;
    private bool deathCoroutineIsRunning;
    void OnDestroy()
    {
        
    }
    void Start()
    {
        currentScene = SceneManager.GetActiveScene();
        playerSettings = FindFirstObjectByType<PlayerSettings>();
        proceduralGeneration = FindFirstObjectByType<ProceduralGeneration>();
        itemSwitcher = FindAnyObjectByType<ItemSwitcher>();
        volume = FindFirstObjectByType<Volume>();
        volume.profile.TryGet<Bloom>(out bloom);
        truck = FindFirstObjectByType<Truck>();
        timer = FindFirstObjectByType<Timer>();
        collectionZone = FindFirstObjectByType<CollectionZone>();
        nextbot = FindAnyObjectByType<Nextbot>();

        sensitivitySlider.value = playerSettings.sensitivity;
        grassDistanceSlider.value = playerSettings.grassDistance;
        died = false;
        pauseMenu = false;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        health = 100;
        loadingTimer = 10;
        bar1.anchoredPosition = new Vector2(-1080, 0);
        bar2.anchoredPosition = new Vector2(1080, 0);
        softlockTimer = 15;

        deathText.gameObject.SetActive(false);

        if(currentScene.name == "Field")
        {
            if(proceduralGeneration.PlaceGrass) loading = true;
           RenderSettings.skybox = sky;
           fogMaterial.SetFloat("_MaxFog", 0f);
        }
        else
        {
            loading = false;
            RenderSettings.skybox = darkSky;
            fogMaterial.SetFloat("_MaxFog", 1f);
        }
    }
    void Update()
    {
        frameCount ++;
        deltaTime += Time.deltaTime;
        if(deltaTime >= 1)
        {
            FPS = frameCount / deltaTime;
            frameCount = 0;
            deltaTime -= 1;
        }
        
        displayFPS.text = "FPS: " + FPS.ToString("F1");

        playerSettings.sensitivity = sensitivitySlider.value;
        playerSettings.grassDistance = grassDistanceSlider.value;

        proceduralGeneration.maxDrawDistance = grassDistanceSlider.value;

        if (playerInput.escapePressed && !died)
        {
            pauseMenu = !pauseMenu;
        }
        if (pauseMenu || died)
        {
            PauseMenu();
        }
        else if (!died && !pauseMenu)
        {
            mainMenuButton.gameObject.SetActive(false);
            sensitivitySlider.gameObject.SetActive(false);
            sensitivityText.gameObject.SetActive(false);
            grassDistanceSlider.gameObject.SetActive(false);
            grassDistanceText.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Displays();
        }
        if(pauseMenu||died)
        {
            Time.timeScale = 0;
        } else
        {
            Time.timeScale = 1;
        }

        if(loading && loadingTimer > 0)
        {
            playerInput.canGiveInput = false;
            Cursor.lockState = CursorLockMode.None;
            blackScreen.gameObject.SetActive(true);
            loadingText.gameObject.SetActive(true);
            instructionText.gameObject.SetActive(false);
            loadingTimer -= Time.deltaTime;
            loadingText.text = "loading: " + ((10-loadingTimer)*10).ToString("F0") + "%";
        }
        else
        {
            playerInput.canGiveInput = true;
            loading = false;
            if(!died) blackScreen.gameObject.SetActive(false);
            loadingText.gameObject.SetActive(false);
            instructionText.gameObject.SetActive(true);
        }

        Animation();

        Instructions();

        if(player.transform.position.y < -20 && !deathCoroutineIsRunning)
            StartCoroutine(DeathScreen("looks like you slipped there"));

        if(currentScene.name == "Infinite" && collectionZone.Count >= 20)
            StartCoroutine(DeathScreen("thats a lot of boxes man."));
        if(nextbot == null && currentScene.name == "Infinite" && timer.time > 0)
        {
            StartCoroutine(DeathScreen("murdah on my mind."));
        }
        else if(nextbot == null && currentScene.name == "Infinite")
        {
            Destroy(portal);
            softlock = true;
        }
        else if (transform.position.z > 499 &&
            transform.position.x > -25 && transform.position.y < 25
            && currentScene.name == "Infinite")
        {
            SceneManager.LoadScene("Purgatory");
        }
        if(softlock)
        {
            softlockTimer -= Time.unscaledDeltaTime;
        }
        if(softlockTimer <= 0)
        {
            StartCoroutine(DeathScreen("revenge isnt the answer."));
        }
    }
    private void Instructions()
    {
        if(currentScene.name == "Field")
        {
            instructionText.rectTransform.anchoredPosition = new Vector2(-545, 505);
            if(truck.loaded) instructionText.text = "good work";
                else if(truck.docked) instructionText.text = "load the truck with 5 boxes";
                else instructionText.text = "go to the depot";
        }
        if(currentScene.name == "Infinite")
        {
            instructionText.rectTransform.anchoredPosition = new Vector2(
                bar1.anchoredPosition.x + 535, 505);
            if(timer.time <= 0) instructionText.text = "run";
                else instructionText.text = "load 20 boxes onto the platform";
        }
    }
    private void Animation()
    {
        if(currentScene.name == "Infinite")
        {
            bar1.anchoredPosition = Vector3.MoveTowards(bar1.anchoredPosition, 
                new Vector2(-840, 0), 2.5f);
            bar2.anchoredPosition = Vector3.MoveTowards(bar2.anchoredPosition, 
                new Vector2(840, 0), 2.5f);
        }
        slot1.sprite = itemSwitcher.icons[0];
        slot2.sprite = itemSwitcher.icons[1];
        slot3.sprite = itemSwitcher.icons[2];

        if(currentScene.name == "Field")
        {
            if(truck.exploded)
            {
                bloom.intensity.value += Time.deltaTime * 2;
                bloom.threshold.value -= Time.deltaTime;
            }
            if(bloom.intensity.value > 10)
            {
                SceneManager.LoadScene("Infinite");
            }
        }
    }
    public void TakenDamage(float damage)
    {
        health -= damage;
        Debug.Log(health);
    }
    public IEnumerator DeathScreen(string causeOfDeath)
    {
        deathCoroutineIsRunning = true;
        died = true;
        blackScreen.gameObject.SetActive(true);

        yield return new WaitForSecondsRealtime(1);

        deathText.gameObject.SetActive(true);
        deathText.text = causeOfDeath;

        yield return new WaitForSecondsRealtime(3);

        SceneManager.LoadScene("MainMenu");
        deathCoroutineIsRunning = false;
    }
    private void PauseMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        sensitivityText.text = "Sensitivity: " + sensitivitySlider.value.ToString("F0");
        grassDistanceText.text = "Grass distance: " + grassDistanceSlider.value.ToString("F0");
        mainMenuButton.gameObject.SetActive(true);
        sensitivitySlider.gameObject.SetActive(true);
        sensitivityText.gameObject.SetActive(true);
        grassDistanceSlider.gameObject.SetActive(true);
        grassDistanceText.gameObject.SetActive(true);
        displaySpeed.enabled = false;
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    private void Displays()
    {
        speed = player.rb.linearVelocity.magnitude;
        displaySpeed.text = speed.ToString("F0");
        displaySpeed.enabled = true;
    }
}