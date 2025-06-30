using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RopeFragment : MonoBehaviour
{
    private HashSet<Collider2D> currentCollisions = new HashSet<Collider2D>();
    private int currentLevel = 0; // 0 = ungrounded, 1 = Grounded, 2 = Grounded2, etc.

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision.collider);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        RemoveCollision(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessCollision(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        RemoveCollision(other);
    }

    void ProcessCollision(Collider2D other)
    {
        if (other.CompareTag("ground") || other.TryGetComponent<RopeFragment>(out _))
        {
            currentCollisions.Add(other);
            RecalculateLevel();
        }
    }

    void RemoveCollision(Collider2D other)
    {
        if (currentCollisions.Contains(other))
        {
            currentCollisions.Remove(other);
            RecalculateLevel();
        }
    }

    void RecalculateLevel()
    {
        int newLevel = CalculateNewLevel();
        
        if (newLevel != currentLevel)
        {
            currentLevel = newLevel;
            UpdateTag();
            // NotifyConnectedFragments();
        }
    }

    int CalculateNewLevel()
    {
        // Check for direct ground contact
        if (currentCollisions.Any(c => c.CompareTag("ground")))
            return 1;

        // Get valid rope fragment connections
        var fragments = currentCollisions
            .Where(c => c.TryGetComponent<RopeFragment>(out var frag) && frag.currentLevel > 0)
            .Select(c => c.GetComponent<RopeFragment>());

        if (!fragments.Any())
            return 0; // No valid connections

        return fragments.Min(f => f.currentLevel) + 1;
    }

    void UpdateTag()
    {
        gameObject.tag = currentLevel switch
        {
            1 => "Grounded",
            2 => "Grounded2",
            3 => "Grounded3",
            _ => "Untagged"
        };
    }

    void NotifyConnectedFragments()
    {
        foreach (var collider in currentCollisions.ToList())
        {
            if (collider.TryGetComponent<RopeFragment>(out var fragment))
            {
                fragment.RecalculateLevel();
            }
        }
    }

    public void ForceRecalculation()
    {
        RecalculateLevel();
    }
}