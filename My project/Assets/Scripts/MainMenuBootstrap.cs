using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class MainMenuBootstrap : MonoBehaviour
{
    private const string GameSceneName = "DesktopSurvivorsPrototype";
    private const string MasterVolumeKey = "survivors_master_volume";
    private const string MusicVolumeKey = "survivors_music_volume";
    private const string SfxVolumeKey = "survivors_sfx_volume";

    private SurvivorsSaveData saveData;
    private string selectedCharacterId;
    private GameObject characterPanel;
    private GameObject settingsPanel;
    private Text pistolLabel;
    private Text shotgunLabel;
    private Text bestKillText;
    private Image pistolPortrait;
    private Image shotgunPortrait;
    private Image shotgunWeapon;
    private Button shotgunButton;
    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioClip clickClip;
    private bool built;

    private void Awake()
    {
        BuildMenu();
    }

    private void OnEnable()
    {
        BuildMenu();
        EnsureMenuMusicPlaying();
    }

    private void BuildMenu()
    {
        if (built)
            return;

        built = true;
        ClearGeneratedObjects();
        Time.timeScale = 1f;

        saveData = SurvivorsSaveSystem.Load();
        selectedCharacterId = string.IsNullOrEmpty(saveData.selectedCharacterId)
            ? SurvivorsCharacterIds.Pistol
            : saveData.selectedCharacterId;

        if (selectedCharacterId == SurvivorsCharacterIds.Shotgun && !saveData.shotgunSurvivorUnlocked)
            selectedCharacterId = SurvivorsCharacterIds.Pistol;

        CreateCamera();
        CreateAudio();
        CreateCanvas();
        CreateEventSystem();
        ApplyVolumes();
        EnsureMenuMusicPlaying();
    }

    private void ClearGeneratedObjects()
    {
        DestroyGeneratedObject("Main Menu Camera");
        DestroyGeneratedObject("Menu Audio");
        DestroyGeneratedObject("Main Menu Canvas");
        DestroyGeneratedObject("EventSystem");
    }

    private void DestroyGeneratedObject(string objectName)
    {
        var existing = GameObject.Find(objectName);
        if (existing == null || existing == gameObject)
            return;

        if (Application.isPlaying)
            Destroy(existing);
        else
            DestroyImmediate(existing);
    }

    private void CreateCamera()
    {
        var cameraObject = new GameObject("Main Menu Camera");
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.025f, 0.028f, 0.025f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
    }

    private void CreateAudio()
    {
        clickClip = CreateClickClip();

        var audioObject = new GameObject("Menu Audio");
        sfxSource = audioObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = audioObject.AddComponent<AudioSource>();
        musicSource.clip = CreateMenuMusicClip();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        if (Application.isPlaying)
            musicSource.Play();
    }

    private void CreateCanvas()
    {
        var canvasObject = new GameObject("Main Menu Canvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        var root = canvasObject.transform;
        BuildBackground(root);
        BuildTitle(root);
        BuildMainButtons(root);
        BuildCharacterSelect(root);
        BuildSettings(root);
        CloseCharacterSelect();
        CloseSettings();
        RefreshCharacters();
    }

    private void BuildBackground(Transform root)
    {
        var background = Resources.Load<Texture2D>("MainMenuBackground");
        if (background != null)
        {
            background.filterMode = FilterMode.Point;
            CreateRawImage(root, "Wasteland Street Background", background, Vector2.zero, Vector2.one, Color.white);
        }
        else
        {
            CreatePanel(root, "Night Wasteland", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.035f, 0.04f, 0.032f, 1f));
        }

        CreatePanel(root, "Top Readability Shade", new Vector2(0f, 0.70f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.02f, 0.02f, 0.02f, 0.28f));
        CreatePanel(root, "Bottom Smoke Shade", Vector2.zero, new Vector2(1f, 0.35f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.018f, 0.015f, 0.30f));
        CreatePanel(root, "Left Blood Shade", Vector2.zero, new Vector2(0.20f, 1f), Vector2.zero, Vector2.zero, new Color(0.16f, 0.02f, 0.018f, 0.18f));
    }

    private void BuildTitle(Transform root)
    {
        var titleBoundsMin = new Vector2(0.285f, 0.710f);
        var titleBoundsMax = new Vector2(0.715f, 0.925f);
        const string titleText = "WASTELAND\nSURVIVORS";

        CreateText(root, "Title Outline Left", titleBoundsMin, titleBoundsMax, new Vector2(-5f, -1f), Vector2.zero, 72, TextAnchor.MiddleCenter, titleText, new Color(0.025f, 0.006f, 0.006f, 1f));
        CreateText(root, "Title Outline Right", titleBoundsMin, titleBoundsMax, new Vector2(5f, -1f), Vector2.zero, 72, TextAnchor.MiddleCenter, titleText, new Color(0.025f, 0.006f, 0.006f, 1f));
        CreateText(root, "Title Heavy Shadow", titleBoundsMin, titleBoundsMax, new Vector2(7f, -8f), Vector2.zero, 72, TextAnchor.MiddleCenter, titleText, new Color(0.06f, 0.008f, 0.01f, 0.95f));
        var title = CreateText(root, "Title", titleBoundsMin, titleBoundsMax, Vector2.zero, Vector2.zero, 72, TextAnchor.MiddleCenter, titleText, new Color(0.92f, 0.10f, 0.08f, 1f));
        title.fontStyle = FontStyle.Bold;
    }

    private void BuildMainButtons(Transform root)
    {
        CreateMenuButton(root, "Start Button", "게임 시작", new Vector2(0.405f, 0.445f), new Vector2(0.595f, 0.525f), new Color(0.58f, 0.12f, 0.045f, 0.96f), StartGame);
        CreateMenuButton(root, "Character Button", "캐릭터", new Vector2(0.405f, 0.345f), new Vector2(0.595f, 0.425f), new Color(0.16f, 0.145f, 0.105f, 0.96f), OpenCharacterSelect);
        CreateMenuButton(root, "Settings Button", "설정", new Vector2(0.405f, 0.245f), new Vector2(0.595f, 0.325f), new Color(0.16f, 0.145f, 0.105f, 0.96f), OpenSettings);
        CreateMenuButton(root, "Quit Button", "종료", new Vector2(0.405f, 0.145f), new Vector2(0.595f, 0.225f), new Color(0.11f, 0.095f, 0.08f, 0.96f), QuitGame);
    }

    private void BuildCharacterSelect(Transform root)
    {
        characterPanel = CreatePanel(root, "Character Select Panel", new Vector2(0.18f, 0.16f), new Vector2(0.82f, 0.76f), Vector2.zero, Vector2.zero, new Color(0.025f, 0.026f, 0.024f, 0.97f));
        characterPanel.GetComponent<Image>().raycastTarget = true;
        CreatePanel(characterPanel.transform, "Top Line", new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -4f), new Vector2(0f, 8f), new Color(0.64f, 0.12f, 0.05f, 1f));
        CreateText(characterPanel.transform, "Character Title", new Vector2(0.06f, 0.84f), new Vector2(0.56f, 0.97f), Vector2.zero, Vector2.zero, 30, TextAnchor.MiddleLeft, "캐릭터 선택", new Color(0.95f, 0.82f, 0.52f, 1f));
        bestKillText = CreateText(characterPanel.transform, "Best Kills", new Vector2(0.60f, 0.84f), new Vector2(0.92f, 0.97f), Vector2.zero, Vector2.zero, 21, TextAnchor.MiddleRight, "", new Color(0.78f, 0.82f, 0.65f, 1f));

        var pistolButton = CreateCharacterCard(characterPanel.transform, new Vector2(0.08f, 0.18f), new Vector2(0.45f, 0.78f), false, out pistolLabel, out pistolPortrait, out _);
        shotgunButton = CreateCharacterCard(characterPanel.transform, new Vector2(0.55f, 0.18f), new Vector2(0.92f, 0.78f), true, out shotgunLabel, out shotgunPortrait, out shotgunWeapon);

        CreateMenuButton(characterPanel.transform, "Close Character Button", "닫기", new Vector2(0.39f, 0.04f), new Vector2(0.61f, 0.14f), new Color(0.42f, 0.10f, 0.045f, 0.96f), CloseCharacterSelect);
        pistolButton.onClick.AddListener(() => SelectCharacter(SurvivorsCharacterIds.Pistol));
        shotgunButton.onClick.AddListener(() => SelectCharacter(SurvivorsCharacterIds.Shotgun));
    }

    private Button CreateCharacterCard(Transform parent, Vector2 anchorMin, Vector2 anchorMax, bool lockedStyle, out Text label, out Image portrait, out Image weapon)
    {
        var card = CreatePanel(parent, lockedStyle ? "Shotgun Character Card" : "Pistol Character Card", anchorMin, anchorMax, Vector2.zero, Vector2.zero, new Color(0.06f, 0.058f, 0.050f, 0.98f));
        var button = card.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(0.06f, 0.058f, 0.050f, 0.98f);
        colors.highlightedColor = new Color(0.24f, 0.20f, 0.13f, 1f);
        colors.pressedColor = new Color(0.52f, 0.20f, 0.08f, 1f);
        colors.disabledColor = new Color(0.04f, 0.04f, 0.038f, 0.95f);
        button.colors = colors;
        AddFeedback(card);

        CreatePanel(card.transform, "Card Top Line", new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -3f), new Vector2(0f, 6f), lockedStyle ? new Color(0.18f, 0.18f, 0.16f, 1f) : new Color(0.65f, 0.14f, 0.06f, 1f));
        portrait = CreateSpriteImage(card.transform, "Portrait", LoadMenuSprite("Survivor", 32f), new Vector2(0.07f, 0.28f), new Vector2(0.46f, 0.84f), lockedStyle ? new Color(0.36f, 0.38f, 0.36f, 0.72f) : Color.white);
        weapon = CreateSpriteImage(card.transform, "Weapon", lockedStyle ? LoadMenuSprite("Shotgun", 32f) : LoadMenuSprite("Pistol", 32f), new Vector2(0.47f, 0.50f), new Vector2(0.90f, 0.76f), lockedStyle ? new Color(0.30f, 0.30f, 0.28f, 0.55f) : new Color(0.95f, 0.86f, 0.62f, 1f));
        label = CreateText(card.transform, "Label", new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.27f), Vector2.zero, Vector2.zero, 22, TextAnchor.MiddleCenter, "", lockedStyle ? new Color(0.58f, 0.60f, 0.52f, 1f) : new Color(0.95f, 0.86f, 0.62f, 1f));

        if (lockedStyle)
            CreateText(card.transform, "Question Mark", new Vector2(0.48f, 0.33f), new Vector2(0.88f, 0.86f), Vector2.zero, Vector2.zero, 70, TextAnchor.MiddleCenter, "?", new Color(0.62f, 0.66f, 0.58f, 0.65f));

        return button;
    }

    private void BuildSettings(Transform root)
    {
        settingsPanel = CreatePanel(root, "Settings Panel", new Vector2(0.33f, 0.20f), new Vector2(0.67f, 0.76f), Vector2.zero, Vector2.zero, new Color(0.035f, 0.036f, 0.032f, 0.98f));
        settingsPanel.GetComponent<Image>().raycastTarget = true;
        CreatePanel(settingsPanel.transform, "Top Line", new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -4f), new Vector2(0f, 8f), new Color(0.68f, 0.14f, 0.06f, 1f));
        CreateText(settingsPanel.transform, "Settings Title", new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.96f), Vector2.zero, Vector2.zero, 34, TextAnchor.MiddleCenter, "설정", new Color(1f, 0.84f, 0.54f, 1f));
        CreateText(settingsPanel.transform, "Master Label", new Vector2(0.12f, 0.66f), new Vector2(0.88f, 0.73f), Vector2.zero, Vector2.zero, 22, TextAnchor.MiddleLeft, "전체 음량", new Color(0.82f, 0.78f, 0.65f, 1f));
        CreateText(settingsPanel.transform, "Music Label", new Vector2(0.12f, 0.48f), new Vector2(0.88f, 0.55f), Vector2.zero, Vector2.zero, 22, TextAnchor.MiddleLeft, "배경음", new Color(0.82f, 0.78f, 0.65f, 1f));
        CreateText(settingsPanel.transform, "Sfx Label", new Vector2(0.12f, 0.30f), new Vector2(0.88f, 0.37f), Vector2.zero, Vector2.zero, 22, TextAnchor.MiddleLeft, "효과음", new Color(0.82f, 0.78f, 0.65f, 1f));

        var master = CreateSlider(settingsPanel.transform, "Master Slider", new Vector2(0.12f, 0.60f), new Vector2(0.88f, 0.64f), PlayerPrefs.GetFloat(MasterVolumeKey, 0.85f));
        master.onValueChanged.AddListener(SetMasterVolume);
        var music = CreateSlider(settingsPanel.transform, "Music Slider", new Vector2(0.12f, 0.42f), new Vector2(0.88f, 0.46f), PlayerPrefs.GetFloat(MusicVolumeKey, 0.85f));
        music.onValueChanged.AddListener(SetMusicVolume);
        var sfx = CreateSlider(settingsPanel.transform, "Sfx Slider", new Vector2(0.12f, 0.24f), new Vector2(0.88f, 0.28f), PlayerPrefs.GetFloat(SfxVolumeKey, 0.9f));
        sfx.onValueChanged.AddListener(SetSfxVolume);

        CreateMenuButton(settingsPanel.transform, "Close Button", "닫기", new Vector2(0.33f, 0.07f), new Vector2(0.67f, 0.18f), new Color(0.50f, 0.12f, 0.05f, 1f), CloseSettings);
    }

    private Button CreateMenuButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Color color, UnityEngine.Events.UnityAction action)
    {
        var go = CreatePanel(parent, name, anchorMin, anchorMax, Vector2.zero, Vector2.zero, color);
        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = new Color(Mathf.Min(color.r + 0.18f, 1f), Mathf.Min(color.g + 0.10f, 1f), Mathf.Min(color.b + 0.04f, 1f), 1f);
        colors.pressedColor = new Color(Mathf.Max(color.r - 0.14f, 0f), Mathf.Max(color.g - 0.08f, 0f), Mathf.Max(color.b - 0.04f, 0f), 1f);
        button.colors = colors;
        button.onClick.AddListener(action);
        CreateText(go.transform, "Label", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 28, TextAnchor.MiddleCenter, label, new Color(1f, 0.88f, 0.58f, 1f));
        AddFeedback(go);
        return button;
    }

    private Slider CreateSlider(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, float value)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        var rect = root.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        CreatePanel(root.transform, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.11f, 0.10f, 0.085f, 1f));
        var fillArea = CreatePanel(root.transform, "Fill Area", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0f));
        var fill = CreatePanel(fillArea.transform, "Fill", Vector2.zero, new Vector2(value, 1f), Vector2.zero, Vector2.zero, new Color(0.62f, 0.16f, 0.06f, 1f));
        var handle = CreatePanel(root.transform, "Handle", new Vector2(value, 0.5f), new Vector2(value, 0.5f), Vector2.zero, new Vector2(30f, 42f), new Color(0.94f, 0.74f, 0.42f, 1f));

        var slider = root.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private void SelectCharacter(string characterId)
    {
        if (characterId == SurvivorsCharacterIds.Shotgun && !saveData.shotgunSurvivorUnlocked)
            return;

        selectedCharacterId = characterId;
        saveData.selectedCharacterId = selectedCharacterId;
        SurvivorsSaveSystem.Save(saveData);
        RefreshCharacters();
    }

    private void RefreshCharacters()
    {
        if (bestKillText != null)
            bestKillText.text = "최고 처치 " + saveData.bestKillCount;

        if (pistolLabel != null)
            pistolLabel.text = SelectedMark(SurvivorsCharacterIds.Pistol) + "생존자\n권총 / 기본 캐릭터";

        if (shotgunLabel != null)
        {
            shotgunLabel.text = saveData.shotgunSurvivorUnlocked
                ? SelectedMark(SurvivorsCharacterIds.Shotgun) + "브리처\n샷건 / 산탄 사격"
                : "미확인 생존자\n50킬 달성 후 해금";
        }

        if (shotgunButton != null)
            shotgunButton.interactable = saveData.shotgunSurvivorUnlocked;

        if (pistolPortrait != null)
            pistolPortrait.color = selectedCharacterId == SurvivorsCharacterIds.Pistol ? Color.white : new Color(0.78f, 0.78f, 0.72f, 1f);

        if (shotgunPortrait != null)
            shotgunPortrait.color = saveData.shotgunSurvivorUnlocked ? Color.white : new Color(0.36f, 0.38f, 0.36f, 0.72f);

        if (shotgunWeapon != null)
            shotgunWeapon.color = saveData.shotgunSurvivorUnlocked ? new Color(0.95f, 0.86f, 0.62f, 1f) : new Color(0.30f, 0.30f, 0.28f, 0.55f);
    }

    private string SelectedMark(string characterId)
    {
        return selectedCharacterId == characterId ? "> " : "";
    }

    private void OpenCharacterSelect()
    {
        if (characterPanel == null)
            return;

        CloseSettings();
        characterPanel.SetActive(true);
        characterPanel.transform.SetAsLastSibling();
    }

    private void CloseCharacterSelect()
    {
        if (characterPanel != null)
            characterPanel.SetActive(false);
    }

    private void StartGame()
    {
        if (!Application.isPlaying)
            return;

        saveData.selectedCharacterId = selectedCharacterId;
        SurvivorsSaveSystem.Save(saveData);
        GameLaunchConfig.StartImmediately = true;
        SceneManager.LoadScene(GameSceneName);
    }

    private void OpenSettings()
    {
        if (settingsPanel == null)
            return;

        CloseCharacterSelect();
        settingsPanel.SetActive(true);
        settingsPanel.transform.SetAsLastSibling();
    }

    private void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        UpdateMusicVolume();
    }

    private void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        UpdateMusicVolume();
    }

    private void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
        if (sfxSource != null)
            sfxSource.volume = value;
    }

    private void ApplyVolumes()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(MasterVolumeKey, 0.85f));
        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, 0.85f));
        SetSfxVolume(PlayerPrefs.GetFloat(SfxVolumeKey, 0.9f));
    }

    private void UpdateMusicVolume()
    {
        if (musicSource == null)
            return;

        musicSource.volume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.85f) * 0.90f;
    }

    private void EnsureMenuMusicPlaying()
    {
        if (!Application.isPlaying || musicSource == null || musicSource.isPlaying)
            return;

        musicSource.Play();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return go;
    }

    private Image CreateSpriteImage(Transform parent, string name, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.preserveAspect = true;
        image.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return image;
    }

    private Sprite LoadMenuSprite(string resourceName, float pixelsPerUnit)
    {
        var texture = Resources.Load<Texture2D>("Menu/" + resourceName);
        if (texture == null)
            return resourceName == "Shotgun" ? CreateWeaponSprite(64, 24) : CreateWeaponSprite(48, 24);

        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    private RawImage CreateRawImage(Transform parent, string name, Texture texture, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<RawImage>();
        image.texture = texture;
        image.color = color;
        image.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return image;
    }

    private Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, string initialText, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.text = initialText;
        text.raycastTarget = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return text;
    }

    private void AddFeedback(GameObject go)
    {
        var feedback = go.AddComponent<MenuButtonFeedback>();
        feedback.audioSource = sfxSource;
        feedback.pressClip = clickClip;
    }

    private void CreateEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    private Sprite CreateWeaponSprite(int width, int height)
    {
        var texture = NewTexture(width, height);
        FillRect(texture, 6, 10, width - 14, 15, Color.white);
        FillRect(texture, width - 16, 11, width - 4, 13, Color.white);
        FillRect(texture, 14, 5, 24, 10, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16f);
    }

    private Texture2D NewTexture(int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                texture.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
        }
        return texture;
    }

    private void FillRect(Texture2D texture, int minX, int minY, int maxX, int maxY, Color color)
    {
        for (var y = minY; y < maxY; y++)
        {
            for (var x = minX; x < maxX; x++)
                texture.SetPixel(x, y, color);
        }
    }

    private AudioClip CreateClickClip()
    {
        const int sampleRate = 44100;
        var samples = Mathf.RoundToInt(sampleRate * 0.12f);
        var data = new float[samples];
        for (var i = 0; i < samples; i++)
        {
            var t = (float)i / sampleRate;
            data[i] = Mathf.Sin(t * Mathf.PI * 2f * 720f) * Mathf.Clamp01(1f - t / 0.12f) * 0.25f;
        }

        var clip = AudioClip.Create("Menu Click", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private AudioClip CreateMenuMusicClip()
    {
        const int sampleRate = 44100;
        var samples = sampleRate * 8;
        var data = new float[samples];
        var tones = new[] { 55f, 65.41f, 73.42f, 82.41f };

        for (var i = 0; i < samples; i++)
        {
            var t = (float)i / sampleRate;
            var beat = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(t * Mathf.PI * 2f * 0.5f)), 8f);
            var tone = tones[Mathf.FloorToInt(t * 0.5f) % tones.Length];
            var drone = Mathf.Sin(t * Mathf.PI * 2f * 41.2f) * 0.12f + Mathf.Sin(t * Mathf.PI * 2f * 82.4f) * 0.06f;
            var pulse = Mathf.Sin(t * Mathf.PI * 2f * tone) * 0.10f * beat;
            var grit = Mathf.Sin(t * Mathf.PI * 2f * 27.5f) * 0.04f;
            data[i] = (drone + pulse + grit) * 0.85f;
        }

        var clip = AudioClip.Create("Menu Wasteland Theme", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
