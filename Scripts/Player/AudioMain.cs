using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
public class AudioMain : MonoBehaviour
{
    //sources
    public AudioSource directional;
    public AudioSource musicSource;
    public AudioSource environment;
    public AudioSource dialogueSource;
    public AudioSource footstepSource;
    public AudioSource playerShotsource;
    public AudioSource playerExtraSource;
    public AudioSource truckSource;
    //clips
    public AudioClip grassSteps;
    public AudioClip concreteSteps;
    public AudioClip pistol;
    public AudioClip equip;
    public AudioClip[] ambient;
    public AudioClip depotTheme;
    public AudioClip mainMenuTheme;
    public AudioClip wind;
    public AudioClip slide;
    public AudioClip engine;
    public AudioClip explosion;
    public AudioClip ringing;
    //other
    public UI ui;
    public PlayerMovement player;
    private float footstepVolume;
    private string currentScene;
    private RaycastHit groundHit;
    private bool explosionIsOver;
    void Start()
    {
        ui = FindFirstObjectByType<UI>();
        player = FindAnyObjectByType<PlayerMovement>();

        currentScene = SceneManager.GetActiveScene().name;

        playerShotsource.volume = 0;
        playerExtraSource.volume = 0;
        explosionIsOver = false;
    }
    void Update()
    {
        if(currentScene == "Purgatory" && !musicSource.isPlaying) Music("Purgatory", 0.2f);
        if(currentScene == "Purgatory") return;


        if(currentScene == "MainMenu" && !musicSource.isPlaying) Music("MainMenu", 0.2f);

        if(currentScene == "MainMenu") return;
        if(ui.loading) return;

        if(!ui.loading && !musicSource.isPlaying) Music(currentScene, 0.2f);

        if(currentScene == "Field" && !ui.loading && !environment.isPlaying)
        {
            environment.volume = 1f;
            environment.PlayOneShot(wind);
        }

        if(Physics.Raycast(player.transform.position + Vector3.up*10, Vector3.down, out groundHit, 100))
        {
            if(groundHit.transform == null) return;

            if(groundHit.transform.name == "grass")
            {
                Footsteps(grassSteps);
            }
            else
            {
                Footsteps(concreteSteps);
            }
        }
        else
        {
            footstepSource.volume = 0;
        }

        if(player.state == PlayerMovement.MovementState.sliding && !playerExtraSource.isPlaying)
        {
            playerExtraSource.volume = 0.5f;
            playerExtraSource.PlayOneShot(slide);
        }
        else if(player.state == PlayerMovement.MovementState.sliding)
        {
            playerExtraSource.volume = (player.rb.linearVelocity.magnitude - 3) / 10;
        }
        else
        {
            playerExtraSource.volume = 0;
        }

        Truck();

        if(ui.itemSwitcher.raising && !playerShotsource.isPlaying)
        {
            playerShotsource.volume = 0.5f;
            playerShotsource.PlayOneShot(equip);
        }

        if(ui.died) AudioListener.volume = 0; else AudioListener.volume = 1;
    }
    private void Truck()
    {
        if(currentScene == "Field")
        {
            if(ui.truck.exploded && !explosionIsOver)
            {
                truckSource.Stop();
                truckSource.PlayOneShot(explosion);
                musicSource.Stop();
                musicSource.PlayOneShot(ringing);
                explosionIsOver = true;
            }
            else if(!truckSource.isPlaying && !explosionIsOver)
            {
                truckSource.PlayOneShot(engine);
            }
        }
        if(currentScene == "Infinite")
        {
            if(!truckSource.isPlaying)
            {
                truckSource.PlayOneShot(engine);
            }
        }

    }
    private void Footsteps(AudioClip type)
    {
        footstepSource.clip = type;
        if(player.state == PlayerMovement.MovementState.crouching && player.rb.linearVelocity.magnitude > 1)
        {
            footstepSource.pitch = 0.75f;
            footstepVolume += 0.05f;
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else if (player.state == PlayerMovement.MovementState.walking && player.rb.linearVelocity.magnitude > 1)
        {
            footstepSource.pitch = 1;
            footstepVolume += 0.05f;
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else if(player.state == PlayerMovement.MovementState.running && player.rb.linearVelocity.magnitude > 1)
        {
            footstepSource.pitch = 2;
            footstepVolume += 0.05f;
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else if (footstepSource.isPlaying)
        {

            footstepVolume -= 0.05f;
            if (footstepSource.volume <= 0 && footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }

        footstepVolume = Mathf.Clamp(footstepVolume, 0, 0.2f);
        footstepSource.volume = footstepVolume;
    }
    public void BulletFired(string shotName)
    {
        playerShotsource.Stop();
        playerShotsource.clip = pistol;
        playerShotsource.Play();
        playerShotsource.volume = 0.5f;
    }
    public void Dialogue()
    {

    }
    public void Music(string Scene, float volume)
    {
        musicSource.volume = volume;
        if(Scene == "Purgatory")
        {
            musicSource.PlayOneShot(ambient[0]);
        }
        if(Scene == "Infinite")
        {
            int index = Random.Range(0, ambient.Length);
            musicSource.PlayOneShot(ambient[index]);
        }
        if(Scene == "MainMenu")
        {
            musicSource.PlayOneShot(mainMenuTheme);
        }
    }
}