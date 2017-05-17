using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GGL;

public class PiecePicker : MonoBehaviour
{
    public float pieceHeight = 5f;
    public float rayDistance = 1000f;
    public LayerMask selectionIgnoreLayer;

    private Piece selectedPiece;
    private CheckerBoard board;
    private Vector3 hitPoint;


    void Start()
    {
        board = FindObjectOfType<CheckerBoard>();
    }

    void FixedUpdate()
    {
        CheckSelection();
        MoveSelection();
    }

    void MoveSelection()
    {
        // Check if we have a piece selected
        if (selectedPiece != null)
        {
            // Create a new ray from camera
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            GizmosGL.AddLine(ray.origin, ray.origin + ray.direction * rayDistance, 0.1f, 0.1f, Color.blue, Color.green);
            RaycastHit hit;
            //Raycast to only hit objects that are not pieces
            if (Physics.Raycast(ray, out hit, rayDistance, ~selectionIgnoreLayer))
            {
                // Obtain the hit point
                GizmosGL.color = Color.white;
                GizmosGL.AddSphere(hit.point, .5f);
                // Move the piece to position
                Vector3 piecePos = hit.point + Vector3.up * pieceHeight;
                selectedPiece.transform.position = piecePos;
                hitPoint = hit.point;
            }
            // Check if mouse button was released
            if(Input.GetMouseButtonUp(0))
            {
                // Move piece to hit point
                Piece piece = selectedPiece.GetComponent<Piece>();
                board.PlacePiece(piece, hitPoint);
                // Deselect the piece
                selectedPiece = null;
            }
        }

    }
    void CheckSelection()
    {
        if (selectedPiece != null)
            return;
        // Create a ray from Camera mouse position to world
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        GizmosGL.AddLine(ray.origin, ray.origin + ray.direction * rayDistance, 0.1f, 0.1f, Color.red, Color.blue);
        RaycastHit hit;
        // Check if the player hits mouse button
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray
            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                // Set the selected piece to be the hit object
                selectedPiece = hit.collider.GetComponent<Piece>();
                // If the user did not hit a piece
                if (selectedPiece == null)
                {
                    //Display a Log message
                    Debug.Log("Cannot pick up object" + hit.collider.name);
                }
            }
        }
    }
}
