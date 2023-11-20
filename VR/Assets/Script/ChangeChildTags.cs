using UnityEngine;

public class ChangeChildTags : MonoBehaviour
{
    public string newTag; // Tag to assign to child objects

    private int previousChildCount; // Previous child count for comparison

    private void Start()
    {
        // Get the Transform component of the parent GameObject
        Transform parentTransform = transform;

        // Assign the new tag to the parent object and child objects recursively
        AssignChildTagsRecursive(parentTransform, newTag);

        // Store the initial child count for comparison
        previousChildCount = CountChildObjectsRecursive(parentTransform);
    }

    private void Update()
    {
        // Get the Transform component of the parent GameObject
        Transform parentTransform = transform;

        // Calculate the current child count recursively
        int currentChildCount = CountChildObjectsRecursive(parentTransform);

        // Check if the child count has changed
        if (currentChildCount != previousChildCount)
        {
            // Assign tags to child objects recursively
            AssignChildTagsRecursive(parentTransform, newTag);

            // Update the previous child count
            previousChildCount = currentChildCount;
        }
    }

    private void AssignChildTagsRecursive(Transform parentTransform, string tag)
    {
        // Assign the tag to the parent object
        parentTransform.gameObject.tag = tag;

        // Iterate through all child objects
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform childTransform = parentTransform.GetChild(i);

            // Recursively assign tags to the child's children
            AssignChildTagsRecursive(childTransform, tag);
        }
    }

    private int CountChildObjectsRecursive(Transform parentTransform)
    {
        int count = 0;

        // Iterate through all child objects
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            Transform childTransform = parentTransform.GetChild(i);

            // Increment the count for the current child object
            count++;

            // Recursively count child objects
            count += CountChildObjectsRecursive(childTransform);
        }

        return count;
    }
}
