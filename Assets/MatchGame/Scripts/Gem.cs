using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour {

    public Board board;

    public int gemType;

    public int xIndex;
    public int yIndex;

	void Start ()
    {

	}

    private void Swap(int x,int y)
    {

    }

    private void Cascade()
    {

    }

    public void MoveTo(int x, int y)
    {
        StartCoroutine(MoveObjectTo(x,y));
    }

    IEnumerator MoveObjectTo(int x,int y)
    {
        float time = board.gameSpeed;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(x, y, 0);

        bool moveEnd = false;

        float elapsedTime = 0.0f;

        while (!moveEnd)
        {
            if ((endPos - transform.position).magnitude < 0.01f)
            {
                //Debug.Log(x+" , "+y+"MoveFinished");
                moveEnd = true;
                transform.position = endPos;
                board.gems[x, y] = this;
                xIndex = x;
                yIndex = y;
            }

            elapsedTime += Time.deltaTime;

            float lerpValue = elapsedTime / time;

            transform.position = Vector3.Lerp(startPos, endPos, lerpValue);

            yield return null;
        }
    }

    public void Fall(int y)
    {
        board.gems[xIndex, yIndex] = null;
        board.gems[xIndex, y] = this;
        //Debug.Log("Fall To :" + y);
        StartCoroutine(FallTo(y));
    }

    IEnumerator FallTo(int y)
    {
        int distance = yIndex - y;
        float time = board.gameSpeed* distance;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, -distance, 0);

        bool moveEnd = false;

        float elapsedTime = 0.0f;

        while (!moveEnd)
        {
            if ((endPos - transform.position).magnitude < 0.01f)
            {
                moveEnd = true;
                //Debug.Log("Fall To :" + y + " End");
                yIndex = y;
                transform.position = endPos;//new Vector3(xIndex,y,0);
                continue;
            }

            elapsedTime += Time.deltaTime;

            float lerpValue = elapsedTime / time;

            transform.position = Vector3.Lerp(startPos, endPos, lerpValue);

            yield return null;
        }
    }

    public void Cascade(int y,int wait)
    {
        board.gems[xIndex, y] = this;
        //Debug.Log("Fall To :" + y);
        StartCoroutine(CascadeRoutine(y,wait));
    }

    IEnumerator CascadeRoutine(int y, int wait)
    {
        int distance = board.boardHeigth - y;
        float time = board.gameSpeed * distance;
        transform.position = new Vector3(xIndex, board.boardHeigth, 0);
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, -distance, 0);

        bool moveEnd = false;

        float elapsedTime = 0.0f;

        yield return new WaitForSeconds(wait*board.gameSpeed);

        while (!moveEnd)
        {
            if ((endPos - transform.position).magnitude < 0.01f)
            {
                moveEnd = true;
                //Debug.Log("Fall To :" + y + " End");
                yIndex = y;
                transform.position = endPos;//new Vector3(xIndex,y,0);
                continue;
            }

            elapsedTime += Time.deltaTime;

            float lerpValue = elapsedTime / time;

            transform.position = Vector3.Lerp(startPos, endPos, lerpValue);

            yield return null;
        }
    }


}
