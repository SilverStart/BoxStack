// PROTOTYPE - NOT FOR PRODUCTION
// Question: Does a 2D/2.5D parcel-box stacking loop fit the Toss app-in-app concept and AI PNG asset pipeline?
// Date: 2026-05-02

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class BoxStackPrototype : MonoBehaviour
{
    private const int TargetBoxes = 8;
    private const float MoveRange = 2.45f;
    private const float MoveSpeed = 1.85f;
    private const float BoxSize = 1.0f;
    private const float DropSettleSeconds = 1.0f;
    private const float LostHeight = -4.0f;
    private const float LostHorizontalDistance = 4.0f;
    private const float CameraBaseY = 2.5f;
    private const float CameraYOffset = 2.2f;

    private readonly List<GameObject> _placedBoxes = new List<GameObject>();

    private GameObject _activeBox;
    private GameObject _droppingBox;
    private Camera _camera;
    private Sprite _boxSprite;
    private Sprite _floorSprite;
    private Color _activeTint = new Color(1.0f, 0.82f, 0.45f);
    private Color _placedTint = new Color(0.86f, 0.62f, 0.34f);
    private PrototypeState _state;
    private float _spawnHeight;
    private float _moveStartedAt;
    private float _cameraVelocityY;
    private int _attempts;
    private string _statusText = "READY";

    private enum PrototypeState
    {
        Playing,
        ResolvingDrop,
        Won,
        Failed
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<BoxStackPrototype>() != null)
        {
            return;
        }

        new GameObject("BoxStack 2D Prototype").AddComponent<BoxStackPrototype>();
    }

    private void Start()
    {
        _boxSprite = CreateParcelBoxSprite();
        _floorSprite = CreateSolidSprite(new Color(0.16f, 0.18f, 0.22f));
        _camera = EnsureCamera();
        CreateFloor();
        RestartGame();
    }

    private void Update()
    {
        if (RestartPressed())
        {
            RestartGame();
            return;
        }

        if (_state == PrototypeState.Playing)
        {
            MoveActiveBox();

            if (DropPressed())
            {
                DropActiveBox();
            }

            if (AnyBoxLost())
            {
                EndRun(false, "STACK LOST");
            }
        }

        UpdateCamera();
    }

    private void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        var shadowStyle = new GUIStyle(style)
        {
            normal = { textColor = new Color(0f, 0f, 0f, 0.55f) }
        };

        string line = $"BOXSTACK  {_placedBoxes.Count}/{TargetBoxes}  {_statusText}";
        var rect = new Rect(0f, 18f, Screen.width, 42f);
        GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), line, shadowStyle);
        GUI.Label(rect, line, style);
    }

    private static Camera EnsureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            var cameraObject = new GameObject("Prototype Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 4.7f;
        camera.transform.position = new Vector3(0f, CameraBaseY, -10f);
        camera.transform.rotation = Quaternion.identity;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.07f, 0.09f, 0.12f);
        return camera;
    }

    private void CreateFloor()
    {
        var floor = new GameObject("Prototype 2D Floor");
        floor.transform.position = new Vector3(0f, -0.65f, 0f);
        floor.transform.localScale = new Vector3(6.2f, 0.35f, 1f);

        var renderer = floor.AddComponent<SpriteRenderer>();
        renderer.sprite = _floorSprite;
        renderer.sortingOrder = -5;

        var collider = floor.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
    }

    private void RestartGame()
    {
        StopAllCoroutines();

        if (_activeBox != null)
        {
            Destroy(_activeBox);
        }

        if (_droppingBox != null)
        {
            Destroy(_droppingBox);
        }

        foreach (GameObject box in _placedBoxes)
        {
            if (box != null)
            {
                Destroy(box);
            }
        }

        _placedBoxes.Clear();
        _activeBox = null;
        _droppingBox = null;
        _cameraVelocityY = 0f;
        _attempts++;
        _state = PrototypeState.Playing;
        _statusText = $"RUN {_attempts}";
        SpawnNextBox();
    }

    private void SpawnNextBox()
    {
        _spawnHeight = 0.45f + (_placedBoxes.Count * BoxSize) + 2.15f;
        _moveStartedAt = Time.time;

        _activeBox = CreatePrototypeBox($"Prototype Parcel Box {_placedBoxes.Count + 1}", _activeTint);
        _activeBox.transform.position = new Vector3(-MoveRange, _spawnHeight, 0f);
    }

    private GameObject CreatePrototypeBox(string boxName, Color tint)
    {
        var box = new GameObject(boxName);
        box.transform.localScale = Vector3.one * BoxSize;

        var renderer = box.AddComponent<SpriteRenderer>();
        renderer.sprite = _boxSprite;
        renderer.color = tint;
        renderer.sortingOrder = 10 + _placedBoxes.Count;

        var collider = box.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.96f, 0.96f);

        var body = box.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 2.4f;
        body.mass = 1f;
        body.linearDamping = 0.25f;
        body.angularDamping = 0.4f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        return box;
    }

    private void MoveActiveBox()
    {
        if (_activeBox == null)
        {
            return;
        }

        float elapsed = Time.time - _moveStartedAt;
        float x = Mathf.Sin(elapsed * MoveSpeed) * MoveRange;
        _activeBox.transform.position = new Vector3(x, _spawnHeight, 0f);
    }

    private void UpdateCamera()
    {
        if (_camera == null)
        {
            return;
        }

        float highestBoxY = GetHighestBoxY();
        GameObject focusBox = _activeBox != null ? _activeBox : _droppingBox;
        float activeY = focusBox != null ? focusBox.transform.position.y : highestBoxY + 1f;
        float targetY = Mathf.Max(CameraBaseY, Mathf.Lerp(highestBoxY + CameraYOffset, activeY + 1.15f, 0.55f));
        float nextY = Mathf.SmoothDamp(_camera.transform.position.y, targetY, ref _cameraVelocityY, 0.18f);

        _camera.transform.position = new Vector3(0f, nextY, -10f);
        _camera.transform.rotation = Quaternion.identity;
    }

    private float GetHighestBoxY()
    {
        float highest = 0.5f;

        for (int i = 0; i < _placedBoxes.Count; i++)
        {
            GameObject box = _placedBoxes[i];
            if (box != null)
            {
                highest = Mathf.Max(highest, box.transform.position.y);
            }
        }

        return highest;
    }

    private void DropActiveBox()
    {
        if (_activeBox == null)
        {
            return;
        }

        _state = PrototypeState.ResolvingDrop;
        _statusText = "DROP";

        var body = _activeBox.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        StartCoroutine(ResolveDrop(_activeBox));
        _droppingBox = _activeBox;
        _activeBox = null;
    }

    private IEnumerator ResolveDrop(GameObject droppedBox)
    {
        yield return new WaitForSeconds(DropSettleSeconds);

        if (droppedBox == null || BoxIsLost(droppedBox))
        {
            _droppingBox = null;
            EndRun(false, "MISSED");
            yield break;
        }

        var renderer = droppedBox.GetComponent<SpriteRenderer>();
        renderer.color = _placedTint;
        _placedBoxes.Add(droppedBox);
        _droppingBox = null;

        if (_placedBoxes.Count >= TargetBoxes)
        {
            EndRun(true, "STACK COMPLETE");
            yield break;
        }

        _state = PrototypeState.Playing;
        _statusText = $"RUN {_attempts}";
        SpawnNextBox();
    }

    private void EndRun(bool won, string status)
    {
        if (_state == PrototypeState.Won || _state == PrototypeState.Failed)
        {
            return;
        }

        _state = won ? PrototypeState.Won : PrototypeState.Failed;
        _statusText = status;

        if (_activeBox != null)
        {
            Destroy(_activeBox);
            _activeBox = null;
        }

        _droppingBox = null;
    }

    private bool AnyBoxLost()
    {
        for (int i = 0; i < _placedBoxes.Count; i++)
        {
            if (BoxIsLost(_placedBoxes[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool BoxIsLost(GameObject box)
    {
        if (box == null)
        {
            return true;
        }

        Vector3 position = box.transform.position;
        return position.y < LostHeight || Mathf.Abs(position.x) > LostHorizontalDistance;
    }

    private static Sprite CreateParcelBoxSprite()
    {
        const int size = 96;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };

        Color cardboard = new Color(0.76f, 0.49f, 0.23f, 1f);
        Color side = new Color(0.68f, 0.39f, 0.18f, 1f);
        Color top = new Color(0.88f, 0.63f, 0.34f, 1f);
        Color tape = new Color(0.38f, 0.24f, 0.12f, 1f);
        Color edge = new Color(0.12f, 0.08f, 0.05f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool outside = x < 3 || x > size - 4 || y < 3 || y > size - 4;
                bool topBand = y > size - 28;
                bool sideBand = x > size - 24;
                bool verticalTape = x >= 43 && x <= 52;
                bool horizontalTape = y >= 46 && y <= 53;
                bool diagonalTop = topBand && Mathf.Abs((x - 48) - ((y - 68) * 2)) < 3;

                Color pixel = cardboard;
                if (topBand)
                {
                    pixel = top;
                }
                else if (sideBand)
                {
                    pixel = side;
                }

                if (verticalTape || horizontalTape || diagonalTop)
                {
                    pixel = tape;
                }

                if (outside)
                {
                    pixel = edge;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateSolidSprite(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };

        texture.SetPixel(0, 0, color);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }

    private static bool DropPressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool touch = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        return keyboard || mouse || touch;
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
#endif
    }

    private static bool RestartPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }
}
