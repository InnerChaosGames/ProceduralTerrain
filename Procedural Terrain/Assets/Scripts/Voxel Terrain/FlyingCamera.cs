using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCamera : MonoBehaviour
{
    public Camera camera;
    public WorldGenerator world;
    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + (camera.transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal")), Time.deltaTime * 10f);

        transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), 0, 0));
        camera.transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "Terrain")
                {
                    world.GetChunkFromVector3(hit.transform.position).PlaceTerrain(hit.point);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "Terrain")
                {
                    world.GetChunkFromVector3(hit.transform.position).RemoveTerrain(hit.point);
                }
            }
        }
    }
}
