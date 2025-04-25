using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class BlockCollisionEvent : UnityEvent<Collision2D>
{
}

public class BuildingBlock : MonoBehaviour
{
    public BlockSystem blockSystem;
    public float ropeStartOffset;
    private bool ignoreCollision;
    private SpriteRenderer _spriteRenderer;
    private string _spriteName;
    private bool isGrounded = false;
    private void Start()
    {
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _spriteName = _spriteRenderer.sprite.name;
    }
    /*
        private void OnCollisionEnter2D(Collision2D target)
        {
            if ((target.gameObject.TryGetComponent(out BuildingBlock _) && !ignoreCollision)
             || (target.gameObject.TryGetComponent(out Floor _) && !blockSystem.HasFirstBlockLanded)
             || (target.gameObject.TryGetComponent(out Floor _) && GameState.NoFail))
            {
                ignoreCollision = true;
                blockSystem.OnBlockCollision(target);
            }
            else if ((target.gameObject.TryGetComponent(out Floor floor) && floor.isReal) && !Game.instance.IsGameOver && !GameState.NoFail)
            {
                Game.instance.BlockHitFloor();
            }

            else if ((!floor.isReal))
            {
                ignoreCollision = true;
                blockSystem.OnBlockCollision(target);
            }

            if (GameState.CurrentLevelID == 1 || GameState.CurrentLevelID == 2)
            {
                if (target.gameObject.TryGetComponent(out BuildingBlock buildingBlock) && buildingBlock._spriteName == this._spriteName)
                {
                    ignoreCollision = true;
                    blockSystem.OnBlockCollision(target);
                    Destroy(gameObject);
                    Destroy(target.gameObject);
                }
            }
        }*/


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isGrounded)
        {
            if(collision.gameObject.TryGetComponent(out Floor _))
            {
                Game.instance.BlockHitFloor();
            }
        }
    }
    /*
        private void OnCollisionEnter2D(Collision2D target)
        {
            Debug.Log(target.gameObject);
            // Handle collision with BuildingBlock or Floor objects.
            if ((target.gameObject.TryGetComponent(out BuildingBlock _) && !ignoreCollision)
             || (target.gameObject.TryGetComponent(out Floor _) && !blockSystem.HasFirstBlockLanded)
             || (target.gameObject.TryGetComponent(out Floor _) && GameState.NoFail))
            {
                ignoreCollision = true;
                isGrounded = true;
                blockSystem.OnBlockCollision(target);
                if (GameState.CurrentLevelID == 1 || GameState.CurrentLevelID == 2)
                {
                    if (target.gameObject.TryGetComponent(out BuildingBlock buildingBlock) && buildingBlock._spriteName == this._spriteName)
                    {
                        ignoreCollision = true;
                        blockSystem.OnBlockCollision(target);
                        Destroy(gameObject);
                        Destroy(target.gameObject);
                    }
                }
            }
            else if ((target.gameObject.TryGetComponent(out Floor floor) && floor.isReal) && !Game.instance.IsGameOver && !GameState.NoFail)
            {
                Game.instance.BlockHitFloor();
            }
            else if ((target.gameObject.TryGetComponent(out Floor floor2) && floor2.isReversed) && !Game.instance.IsGameOver && !GameState.NoFail && isGrounded)
            {
                Game.instance.BlockHitFloor();
            }
            else if (!floor.isReal)
            {
                ignoreCollision = true;
                blockSystem.OnBlockCollision(target);
            }

        }*/

    private void OnCollisionEnter2D(Collision2D target)
    {
        Debug.Log(target.gameObject);

        // Handle collision with BuildingBlock or Floor objects.
        if ((target.gameObject.TryGetComponent(out BuildingBlock _) && !ignoreCollision)
         || (target.gameObject.TryGetComponent(out Floor _) && !blockSystem.HasFirstBlockLanded)
         || (target.gameObject.TryGetComponent(out Floor _) && GameState.NoFail))
        {
            ignoreCollision = true;
            isGrounded = true;
            blockSystem.OnBlockCollision(target);

        }
        else if ((target.gameObject.TryGetComponent(out Floor floor) && floor.isReal) && !Game.instance.IsGameOver && !GameState.NoFail)
        {
            Game.instance.BlockHitFloor();
            isGrounded = true; // Ensure the block is grounded when it hits the real floor
        }
        else if ((target.gameObject.TryGetComponent(out Floor floor2) && floor2.isReversed) && !Game.instance.IsGameOver && !GameState.NoFail && isGrounded)
        {
            Game.instance.BlockHitFloor();
        }
        else if ((target.gameObject.TryGetComponent(out Floor floor3) && !floor3.isReal))
        {
            ignoreCollision = true;
            blockSystem.OnBlockCollision(target);
        }

        // Destroy block if it's part of Level 1 or 2
        if (GameState.CurrentLevelID == 1)
        {
            if (target.gameObject.TryGetComponent(out BuildingBlock buildingBlock) && buildingBlock._spriteName == this._spriteName)
            {
                Debug.Log("test");
                Destroy(target.gameObject);
                Destroy(gameObject);
            }
        }
    }

    // Coroutine to destroy the objects after the current frame to avoid issues during collision processing.
    private IEnumerator DestroyAfterCollision(GameObject targetObject)
    {
        // Wait until the next frame to destroy the object
        yield return null;
        Destroy(targetObject);
    }
}
