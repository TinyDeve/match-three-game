using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public enum GameMode
    {
        Play,  //Game wait for input from user
        Moving,//When gems are moving
        NoGame,//When game at uı
    }

    public GameMode gameMode;
    public Board board;
    public UIManager uIManager;
    public Camera mainCamera;

    [Range(0,50)]
    public float xMarginPercentage;
    [Range(0, 50)]
    public float yMarginPercentage;

    int score;
    int leftBAse;
    public int left;
    public int finishAt;



    public GameObject finish;
    public Slider scoreSlider;
    public Text scoreText;
    public Text leftText;


    float m_boardWitdh;
    float m_boardHeigth;

    float m_orthographicSize;

    //Amount of pixel each gem tile is acupied
    float eachTilePixel;
    //Amount of margin for better detection
    [Range (.1f,.4f)]
    public float tileMarginPercentage;

    float screenHeigth;
    float screenWitdh;
    float tilePixel;
    Vector2 orjinPoint;

    void Start ()
    {
        gameMode = GameMode.NoGame;
        m_boardWitdh = board.boardWitdh;
        m_boardHeigth = board.boardHeigth;

        leftBAse = left;

        screenHeigth = Screen.height;
        screenWitdh = Screen.width;

        scoreText.text = score.ToString();
        leftText.text = left.ToString();

        //Camera positon and orthographic size are adjusted
        //To fit board in any type of device
        AdjustCameraAndTiles();
	}

    public void Restart()
    {
        score = 0;
        left = leftBAse;
        scoreText.text = score.ToString();
        leftText.text = left.ToString();
        board.RefillBoard();
    }

    public void ChangeScore(int scoreIncrease)
    {
        score += scoreIncrease;

        scoreText.text = score.ToString();

        scoreSlider.normalizedValue = ((float)finishAt) / score;

        if(score > finishAt)
        {
            uIManager.OpenFinish(true,false);
        }
    }

    public void ChangeLeft()
    {
        left -= 1;

        leftText.text = left.ToString();
    }

    public void CheckLoss()
    {
        //Debug.Log(""+finish.activeSelf);
        if (left == 0 && !finish.activeSelf)
        {
            uIManager.OpenFinish(false, false);
        }
    }


    void AdjustCameraAndTiles()
    {
        mainCamera.transform.position = new Vector3((m_boardWitdh - 1) / 2, (m_boardHeigth - 1) / 2, -10);
        


        float screenRatio = screenHeigth / screenWitdh;

        float xOrthographicSize = ((m_boardWitdh) / 2) * (50 / (50 - xMarginPercentage));

        xOrthographicSize = xOrthographicSize * screenRatio;

        float yOrthographicSize = ((m_boardHeigth) / 2) * (50 / (50 - yMarginPercentage));

        if(yOrthographicSize > xOrthographicSize)
        {
            m_orthographicSize = yOrthographicSize;

            tilePixel = (screenHeigth * (1 - (yMarginPercentage / 50))) / m_boardHeigth;
        }
        else
        {
            m_orthographicSize = xOrthographicSize;

            tilePixel = (screenWitdh * (1 - (xMarginPercentage / 50))) / m_boardWitdh;
        }
        

        orjinPoint.x = (screenWitdh - tilePixel * m_boardWitdh) / 2;
        orjinPoint.y = (screenHeigth - tilePixel * m_boardHeigth) / 2;

        Debug.Log("TilePixel : " + tilePixel + "OrjinPoint :" + orjinPoint.ToString());
        //m_orthographicSize = (yOrthographicSize > xOrthographicSize) ? yOrthographicSize : xOrthographicSize;

        mainCamera.orthographicSize = m_orthographicSize;
    }

    #region Input Handler
    void OnEnable()
    {
        InputManager.SwipeEvent += SwipeHandler;
    }

    void OnDisable()
    {
        InputManager.SwipeEvent -= SwipeHandler;
    }

    void SwipeHandler(Vector2 swipeMovement,Vector2 touchPostion)
    {
        Vector2 moveDirection = Vector2.zero;

        //horizontal
        if (Mathf.Abs(swipeMovement.x) > Mathf.Abs(swipeMovement.y))
        {
            moveDirection = (swipeMovement.x >= 0) ? new Vector2(1,0) : new Vector2(-1, 0);//? "right" : "left";

        }
        // vertical
        else
        {
            moveDirection = (swipeMovement.y >= 0) ? new Vector2(0, 1) : new Vector2(0, -1);//? "up" : "down";
        }

        Vector2 tileTouchPixel  = touchPostion - orjinPoint;

        float xPixelMargin = (tileTouchPixel.x % tilePixel);
        float yPixelMargin = (tileTouchPixel.x % tilePixel);



        if (xPixelMargin<(tilePixel*(1-tileMarginPercentage))
            &&(xPixelMargin>(tilePixel*tileMarginPercentage)))
        {
            if (yPixelMargin < (tilePixel * (1 - tileMarginPercentage))
                        && (yPixelMargin > (tilePixel * tileMarginPercentage)))
            {
                int xTileIndex = (int)(tileTouchPixel.x / tilePixel);
                int yTileIndex = (int)(tileTouchPixel.y / tilePixel);

                //Debug.Log(xTileIndex + " , " + yTileIndex);

                gameMode = GameMode.Moving;
                board.MoveGems(xTileIndex, yTileIndex, moveDirection);
            }
        }



    }
    string SwipeDiagnostic(Vector2 swipeMovement)
    {
        string direction = "";

        // horizontal
        if (Mathf.Abs(swipeMovement.x) > Mathf.Abs(swipeMovement.y))
        {
            direction = (swipeMovement.x >= 0) ? "right" : "left";

        }
        // vertical
        else
        {
            direction = (swipeMovement.y >= 0) ? "up" : "down";

        }
        return direction;
    }
    
    #endregion
}
