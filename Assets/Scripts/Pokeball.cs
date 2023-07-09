using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokeball : MonoBehaviour
{
    public AudioClip capturingClip;
    public AudioClip escapeClip;
    public float[] fase1Ball = { 0.65f, 0.25f, 0.1f, 0f };
    public float[] fase2Ball = { 0.4f, 0.4f, 0.2f, 0f };
    public float[] fase3Ball = { 0.15f, 0.35f, 0.5f, 0.1f };
    public float[] fase4Ball = { 0.05f, 0.15f, 0.3f, 0.5f };
    public int[] ballMultipliers = { 1, 2, 4, 100 };
    public Sprite[] sprites = new Sprite[4];

    private float[] _currentFase = new float[4];
    private PokeballType _ballType = PokeballType.NORMAL;

    


    private const float TIME_TO_ESCAPE = 8000; // 8s based on audio
    private const int BASE_MIN_CAPTURES = 3;
    private const int BASE_MAX_CAPTURES = 6;

    private float timeSinceCapture = 0f;
    private int escapeCount = 0;
    private bool isCapturing = false;
    private Coroutine capturingCoroutine;
    private AudioSource _audioSource;
    private SpriteRenderer _spriteRenderer;
    private PlayerControls _playerControls;
    private GameManager _gameManager;

    private int _escapeCountNeeded = 0;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        _spriteRenderer.enabled = false;
    }

    private void OnEnable()
    {
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    private void Update()
    {
        if (isCapturing)
        {
            this.CheckCapturing();
        }
    }

    private void CheckCapturing()
    {
        bool inputEscape = _playerControls.Overworld.EscapePokeball.WasPerformedThisFrame();
        timeSinceCapture += Time.deltaTime;

        if (inputEscape && timeSinceCapture < TIME_TO_ESCAPE)
        {
            escapeCount++;
        }

        if (escapeCount >= _escapeCountNeeded)
        {
            StartCoroutine(StopCapture());
        }
    }

    public void StartCapture(int accumulatedDamage)
    {
        int min = BASE_MIN_CAPTURES;
        int max = BASE_MAX_CAPTURES;

        int countFrames = Time.frameCount;
        _ballType = PokeballType.NORMAL;
        if (accumulatedDamage > 0)
        {
            float r = Random.value;
            if(countFrames > 36000)
            {
                _currentFase = fase4Ball;
            }
            else if (countFrames > 18000)
            {
                _currentFase = fase3Ball;
            }
            else if (countFrames > 9000)
            {
                _currentFase = fase2Ball;
            }
            else
            {
                _currentFase = fase1Ball;
            }
            float accumulatedProb = 0f;
            for (int i = 0; i < 4; i++)
            {
                accumulatedProb += _currentFase[i];
                if (r < accumulatedProb)
                {
                    _ballType = (PokeballType)i;
                    break;
                }
            }
            float multiplier = 1f;
            multiplier += accumulatedDamage * 0.5f;
            multiplier *= ballMultipliers[(int)_ballType];
            min = (int)Mathf.Ceil(min * multiplier);
            max = (int)Mathf.Ceil(max * multiplier);
            _escapeCountNeeded = Random.Range(min, max+1);
        }
        else
        {
            _escapeCountNeeded = Random.Range(3, 5);
        }

        _spriteRenderer.sprite = sprites[(int)_ballType];
        _spriteRenderer.enabled = true;
        _gameManager.SetPlayerActive(false);
        escapeCount = 0;
        isCapturing = true;
        capturingCoroutine = StartCoroutine(Tilt());
    }

    private IEnumerator StopCapture()
    {
        StopCoroutine(capturingCoroutine);
        isCapturing = false;
        transform.eulerAngles = new Vector3(0, 0, 0);
        _audioSource.Stop();
        _audioSource.PlayOneShot(escapeClip);

        yield return new WaitForSeconds(1.8f);

        _spriteRenderer.enabled = false;
        _gameManager.SetPlayerActive(true);
    }

    private IEnumerator Tilt()
    {
        _audioSource.PlayOneShot(capturingClip);

        // First
        transform.eulerAngles = new Vector3(0, 0, 0);
        yield return new WaitForSeconds(0.3f);
        transform.eulerAngles = new Vector3(0, 0, 30);
        yield return new WaitForSeconds(1.5f);

        // Second
        transform.eulerAngles = new Vector3(0, 0, 0);
        yield return new WaitForSeconds(0.3f);
        transform.eulerAngles = new Vector3(0, 0, -30);
        yield return new WaitForSeconds(1.5f);

        // Third
        transform.eulerAngles = new Vector3(0, 0, 0);
        yield return new WaitForSeconds(0.3f);
        transform.eulerAngles = new Vector3(0, 0, 30);
        yield return new WaitForSeconds(1.5f);

        // Fourth
        transform.eulerAngles = new Vector3(0, 0, 0);
        yield return new WaitForSeconds(0.3f);
        transform.eulerAngles = new Vector3(0, 0, -30);
        yield return new WaitForSeconds(1.8f);

        // Captured
        transform.eulerAngles = new Vector3(0, 0, 0);
        // TODO: lose
    }
}

public enum PokeballType
{
    NORMAL=0,
    SUPER=1,
    ULTRA=2,
    MASTER=3
}