using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class Interactor : MonoBehaviour {
    public bool CanInteract { get; set; } = true;
    private ButtonControl southButton;
    private ButtonControl eastButton;

    [SerializeField] private Transform overlapPoint;
    [SerializeField] private float overlapRadius;
    [SerializeField] private LayerMask mask;

    private bool canHold = true;
    private float holdSeconds = 0.5f;
    private float t = 0;

    public int overlappedCount = 0;
    private List<IInteractable> overlappedInteractables = new List<IInteractable>();

    private void Start() {
        southButton = InputManager.Instance.GetGamepad().buttonSouth;
        eastButton = InputManager.Instance.GetGamepad().buttonEast;
    }

    private void Update() {
        CheckForInteractables();
        CheckForInput();
    }

    private void CheckForInteractables() {
        if (overlappedCount > 0) {
            overlappedInteractables[0].HoverDisable(this);
        }

        overlappedInteractables.Clear();

        var colliders = Physics.OverlapSphere(overlapPoint.position, overlapRadius, mask).ToList();
        foreach (var col in colliders) {
            if (col.isTrigger) {
                var curInteractable = col.GetComponent<IInteractable>();
                overlappedInteractables.Add(curInteractable);
            }
        }

        overlappedCount = overlappedInteractables.Count;

        if (overlappedCount > 0) {
            overlappedInteractables[0].HoverEnable(this);
        }
    }

    private void CheckForInput() {
        if (southButton.wasReleasedThisFrame) {
            CanInteract = true;
            ResetHold();
        }

        if (!CanInteract)
            return;

        if (southButton.wasPressedThisFrame) {
            if (!HasInteractable())
                return;

            var hasInteracted = overlappedInteractables[0].PressInteract(this);
            if (hasInteracted) {
                CanInteract = false;
                return;
            }
        }

        if (eastButton.wasPressedThisFrame) {
            if (!HasInteractable())
                return;

            var hasInteracted = overlappedInteractables[0].PressInteractAlt(this);
            if (hasInteracted) {
                CanInteract = false;
                return;
            }
        }

        if (!canHold)
            return;

        if (southButton.isPressed) {
            t += Time.deltaTime;
            if (t >= holdSeconds) {
                if (HasInteractable()) {
                    var hasInteracted = overlappedInteractables[0].LongPressInteract(this);
                    if (hasInteracted) {
                        CanInteract = false;
                        return;
                    }
                }
                canHold = false;
            }
        }
    }

    private bool HasInteractable() {
        if (overlappedInteractables.Count > 0) {
            return true;
        }

        return false;
    }

    private void ResetHold() {
        canHold = true;
        t = 0;
    }

    private void OnDrawGizmos() {
        if (overlapPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(overlapPoint.position, overlapRadius);
    }
}
