using UnityEngine;

[RequireComponent(typeof(OrbiterController))]
public class StoneMonsterAnimatorBridge : MonoBehaviour
{
	[Header("Legacy Animation")]
	public Animation legacyAnimation;
	
	// Animation clip names (matching Animation_Test.cs)
	public const string IDLE = "Anim_Idle";
	public const string RUN = "Anim_Run";
	public const string ATTACK = "Anim_Attack";
	public const string DAMAGE = "Anim_Damage";
	public const string DEATH = "Anim_Death";

	private OrbiterController controller;
	private SimonSaysGame simonGame;
	private bool isMoving = false;

	void Awake()
	{
		controller = GetComponent<OrbiterController>();
		if (legacyAnimation == null)
		{
			legacyAnimation = GetComponentInChildren<Animation>(true);
		}
		
		// Debug logging
		Debug.Log($"StoneMonsterAnimatorBridge Awake - Legacy Animation: {(legacyAnimation != null ? "Found" : "NOT FOUND")}, Controller: {(controller != null ? "Found" : "NOT FOUND")}");
	}

	void OnEnable()
	{
		if (controller != null)
		{
			controller.OnStartedOrbiting += HandleStartedOrbiting;
			controller.OnReachedTargetSimple += HandleReachedTarget;
		}

		simonGame = FindFirstObjectByType<SimonSaysGame>();
        if (simonGame != null)
        {
            simonGame.OnOrbiterShrink += HandleOrbiterShrink;
            simonGame.OnOrbiterGrow += HandleOrbiterGrow;
        }
	}

	void OnDisable()
	{
		if (controller != null)
		{
			controller.OnStartedOrbiting -= HandleStartedOrbiting;
			controller.OnReachedTargetSimple -= HandleReachedTarget;
		}
	}

	private void HandleStartedOrbiting()
	{
		Debug.Log($"HandleStartedOrbiting called - Legacy Animation: {(legacyAnimation != null ? "Found" : "NULL")}");
		if (!isMoving)
		{
			Play(RUN);
			isMoving = true;
		}
	}

	private void HandleReachedTarget()
	{
		Debug.Log($"HandleReachedTarget called - Legacy Animation: {(legacyAnimation != null ? "Found" : "NULL")}");
		
		// Always fix facing direction when reaching target, regardless of movement state
		FixFacingDirection();
		
		PlayAttack();
		if (isMoving)
		{
			isMoving = false;
		}
	}

	private void HandleOrbiterShrink()
	{
		Debug.Log($"HandleOrbiterShrink called - Legacy Animation: {(legacyAnimation != null ? "Found" : "NULL")}");
		Play(DEATH);
	}

	private void HandleOrbiterGrow()
	{
		Debug.Log($"HandleOrbiterGrow called - Legacy Animation: {(legacyAnimation != null ? "Found" : "NULL")}");
		
		// Force face target when growing (appearing)
		FixFacingDirection();
		
		PlayDamage();
	}

	// Public API for gameplay triggers
	public void PlayAttack()
	{
		Play(ATTACK);
	}

	public void PlayDamage()
	{
		Play(DAMAGE);
	}

	public void PlayDeath()
	{
		Play(DEATH);
	}
	
	// Force the monster to face the current target (useful when already at location)
	public void ForceFaceTarget()
	{
		FixFacingDirection();
	}

	private void Play(string clipName)
	{
		if (legacyAnimation != null && legacyAnimation[clipName] != null)
		{
			legacyAnimation.CrossFade(clipName);
			Debug.Log($"Playing animation: {clipName}");
			
			// Fix facing direction for movement animations
			if (clipName == RUN)
			{
				FixFacingDirection();
			}
		}
		else
		{
			Debug.LogWarning($"Cannot play animation '{clipName}' - Animation component or clip not found");
		}
	}
	
	private void FixFacingDirection()
	{
		// Get the current target direction from OrbiterController
		if (controller != null && controller.GetCurrentTarget() != null)
		{
			Vector3 targetDirection = (controller.GetCurrentTarget().position - transform.position).normalized;
			targetDirection.y = 0; // Keep horizontal only
			
			if (targetDirection != Vector3.zero)
			{
				// Rotate to face the target direction with smooth rotation
				Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
				StartCoroutine(SmoothRotateTo(targetRotation));
			}
		}
	}
	
	private System.Collections.IEnumerator SmoothRotateTo(Quaternion targetRotation)
	{
		Quaternion startRotation = transform.rotation;
		float rotationSpeed = 5f; // Adjust this for faster/slower rotation
		float elapsedTime = 0f;
		
		while (elapsedTime < 1f)
		{
			elapsedTime += Time.deltaTime * rotationSpeed;
			transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime);
			yield return null;
		}
		
		transform.rotation = targetRotation;
	}
}


