using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {

    public Gem[,] gems;

    List<Gem> disappearedGems = new List<Gem>();

    public GameObject gemPrefab;

    public GameManager gm;

    public Sprite[] sprites;

    public float gameSpeed;

    bool isComplete = false;

    int comboTimes = 0;

    public int smallestMatchNumber;
    public int gemTypeNumber;
    public int boardHeigth;
    public int boardWitdh;

    public bool swapBackIfNoMatches = true;


    int maxFallDistance;

    Gem gemClicked;
    Gem gemSwaped;

    void Start()
    {
        gems = new Gem[boardWitdh, boardHeigth];
        FillBoard();
    }

    public void FillBoard()
    {
        for (int i = 0; i < boardWitdh; i++)
        {
            for (int j = 0; j < boardHeigth; j++)
            {
                GameObject gemHolder = Instantiate(gemPrefab, transform) as GameObject;

                gemHolder.name = "Gem : " + i + "," + j;

                Gem gem = gemHolder.GetComponent<Gem>();

                gem.board = this;

                RandomGemAt(gem);

                PlaceGemAt(i, j, gem);

                //..make it directinal for optimization
                while (CheckMatchesAt(gem).Count > 0)
                {
                    RandomGemAt(gem);
                }
            }
        }
    }

    public void RefillBoard()
    {
        for (int i = 0; i < boardWitdh; i++)
        {
            for (int j = 0; j < boardHeigth; j++)
            {
                Gem gem = gems[i, j];
                if(gem == null)
                {
                    continue;
                }
                gem.board = this;

                RandomGemAt(gem);
                

                //..make it directinal for optimization
                while (CheckMatchesAt(gem).Count > 0)
                {
                    RandomGemAt(gem);
                }
            }
        }
    }

    void RandomGemAt(Gem gem)
    {
        int gemType = UnityEngine.Random.Range(0, gemTypeNumber);

        gem.gemType = gemType;

        SpriteRenderer spriteRenderer = gem.gameObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && sprites.Length > gemType)
        {
            spriteRenderer.sprite = sprites[gemType];
        }
        else {
            gem.gemType = 0;
            Debug.LogWarning("Not enough sprites added at Board script");
        }

    }

    void PlaceGemAt(int x, int y, Gem gem)
    {
        gem.transform.position = new Vector3(x, y, 0);

        gem.xIndex = x;
        gem.yIndex = y;


        gems[x, y] = gem;
    }

    #region Move Gems
    IEnumerator SwapGemsBack()
    {
        if (swapBackIfNoMatches && gemClicked != null && gemSwaped != null)
        {
            gemClicked.MoveTo(gemSwaped.xIndex, gemSwaped.yIndex);
            gemSwaped.MoveTo(gemClicked.xIndex, gemClicked.yIndex);
        }
        yield return new WaitForSeconds(gameSpeed);

        if (gm.gameMode == GameManager.GameMode.Moving)
        {
            gm.gameMode = GameManager.GameMode.Play;
        }
        //Debug.Log("check loss");
        gm.CheckLoss();
    }

    public void MoveGems(int x, int y, Vector2 direction)
    {
        int x2 = x + (int)direction.x;
        int y2 = y + (int)direction.y;

        if (!IsInBoard(x2, y2))
        {
            Debug.Log("Out Of Border");
            gm.gameMode = GameManager.GameMode.Play;
            return;
        }
        gemClicked = gems[x, y];
        gemSwaped = gems[x2, y2];

        if (gemClicked != null)
        {
            gemClicked.MoveTo(x2, y2);
        }
        if (gemSwaped != null)
        {
            gemSwaped.MoveTo(x, y);
        }

        gm.ChangeLeft();

        StartCoroutine(MoveGemsRoutine(gemClicked, gemSwaped));

    }

    IEnumerator MoveGemsRoutine(Gem gemClicked, Gem gemSwaped)
    {
        //Debug.Log("Move");
        yield return new WaitForSeconds(gameSpeed);

        //Debug.Log("Move1");
        CheckAllMatches(gemClicked, gemSwaped);
    }
    #endregion

    #region Check Matches

    List<Gem> CheckForMacthesAtDirection(Gem gem, Vector2 direction)
    {
        if (gem == null)
        {
            return new List<Gem>();
        }
        int startX = gem.xIndex;
        int startY = gem.yIndex;

        int gemType = gem.gemType;

        int nextX;
        int nextY;

        List<Gem> matches = new List<Gem>();

        for (int i = 1; i < boardHeigth; i++)
        {
            nextX = startX + (int)direction.x * i;
            nextY = startY + (int)direction.y * i;

            if (!IsInBoard(nextX, nextY))
            {
                break;
            }

            Gem lookGem = gems[nextX, nextY];

            if (lookGem != null && lookGem.gemType == gemType)
            {
                //Debug.Log("Direction" + lookGem.name);
                matches.Add(lookGem);
            }
            else
            {
                break;
            }
        }

        return matches;
    }

    List<Gem> CheckVerticalMatches(Gem gem)
    {
        List<Gem> upMatches = CheckForMacthesAtDirection(gem, new Vector2(0, -1));
        List<Gem> downMatches = CheckForMacthesAtDirection(gem, new Vector2(0, 1));

        return CombineLinks(upMatches, downMatches);
    }

    List<Gem> CheckHorizontalMatches(Gem gem)
    {
        List<Gem> leftMatches = CheckForMacthesAtDirection(gem, new Vector2(-1, 0));
        List<Gem> rigthMatches = CheckForMacthesAtDirection(gem, new Vector2(1, 0));

        return CombineLinks(leftMatches, rigthMatches);
    }

    List<Gem> CheckMatchesAt(Gem gem)
    {
        List<Gem> verticalMatches = CheckVerticalMatches(gem);
        List<Gem> horizontalMatches = CheckHorizontalMatches(gem);

        List<Gem> allMatches = new List<Gem>();

        if (verticalMatches != null)
        {
            if (verticalMatches.Count >= (smallestMatchNumber - 1))
            {
                allMatches = CombineLinks(allMatches, verticalMatches);
            }
        }


        if (horizontalMatches != null)
        {
            if (horizontalMatches.Count >= (smallestMatchNumber - 1))
            {
                allMatches = CombineLinks(allMatches, horizontalMatches);
            }
        }

        allMatches.Add(gem);

        if (allMatches.Count >= (smallestMatchNumber - 1))
        {
            return allMatches;
        }

        return new List<Gem>();
    }

    void CheckAllMatches(Gem gem1, Gem gem2)
    {
        List<Gem> allMatches = new List<Gem>();

        allMatches = CombineLinks(CheckMatchesAt(gem1), CheckMatchesAt(gem2));
        //Debug.Log("" + allMatches.Count);

        if (allMatches.Count >= (smallestMatchNumber - 1))
        {
            //FallGems(DisAppearGems(allMatches));
            Debug.Log("Count Chaeck Matches");
            DisAppearFallAllCascade(allMatches);
        }
        else
        {
            StartCoroutine(SwapGemsBack());
            //NoMatch Swap Back
        }
    }

    #endregion

    #region Fall Disappear Cascade
    void DisAppearFallAllCascade(List<Gem> allMatches)
    {
        StartCoroutine(DisAppearFallCascadeToutine(allMatches));
    }

    IEnumerator DisAppearFallCascadeToutine(List<Gem> allMatches)
    {
        Debug.Log("Count Disappear Matches");
        StartCoroutine(DisappearFall(allMatches));

        comboTimes = 0;

        disappearedGems = new List<Gem>();

        yield return null;
    }

    IEnumerator DisappearFall(List<Gem> allMatches)
    {

        List<int> disGems = new List<int>(); ;
        List<Gem> falledGems = new List<Gem>();
        List<Gem> matches = new List<Gem>();

        Debug.Log("All matches count :" + allMatches.Count);
        yield return new WaitForSeconds(gameSpeed/2);

        isComplete = false;

        while (!isComplete)
        {
            //Disappear found matches
            disGems = DisAppearGems(allMatches);

            yield return new WaitForSeconds(gameSpeed / 2);

            //Fall coloumn that gem disappeared
            falledGems = FallGems(disGems);
            
            Debug.Log("Wait for " + (maxFallDistance * gameSpeed) + "seconds for " + falledGems.Count + " fall gems" );
            //Make combo count there
            comboTimes += 1;
            yield return new WaitForSeconds((maxFallDistance + 1) * gameSpeed);

            maxFallDistance = 0;

            matches = new List<Gem>();

            foreach (Gem fallGem in falledGems)
            {
                matches = CombineLinks(matches, CheckMatchesAt(fallGem));
            }

            Debug.Log("Combo : " + comboTimes + " Match count :" + matches.Count);

            if (matches.Count == 0)
            {
                Debug.Log("Count is zero");
                isComplete = true;
                break;
            }
            else
            {

                yield return StartCoroutine(DisappearFall(matches));
            }
        }
        if (gm.gameMode == GameManager.GameMode.Moving)
        {
            gm.gameMode = GameManager.GameMode.Play;
        }
        //Debug.Log("check loss");
        gm.CheckLoss();
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(Cascade());
        yield return null;
    }

    IEnumerator Cascade ()
    {
        List<int> gemIndexes = new List<int>();

        foreach (Gem gem in disappearedGems)
        {
            if (!gemIndexes.Contains(gem.xIndex))
            {
                gemIndexes.Add(gem.xIndex);
            }
        }

        int gemCount = 0;

        foreach (int i in gemIndexes)
        {
            int wait = 0;
            for (int j = 0; j< boardHeigth; j++)
            {

                //look for null objects
                if (gems[i,j] != null)
                {
                    continue;
                }
                wait += 1;
                gemCount += 1;

                Gem gem = disappearedGems[gemCount-1];

                //reswap object out of border
                int index = 0;

                //Debug.Log("Cascade at : " + i +" Wait : "+wait +" Gem Count : " + (gemCount-1));


                PlaceGemAt(i, j, gem);
                gem.transform.position = new Vector3(i, j, 0);

                do
                {
                    //..make it directinal for optimization
                    RandomGemAt(gem);
                    index += 1;
                    if (index > 1001)
                    {
                        //Debug.LogWarning("Inf�nte Loop");
                        break;
                    }
                } while (CheckMatchesAt(gem).Count > 0);

                gem.transform.GetComponent<SpriteRenderer>().enabled = true;

                //fall to null point
                gem.Cascade(j,wait-1);
            }
        }
        yield return null;
    }

    void ReSwapGem(int i)
    {
    }

    List<int> DisAppearGems(List<Gem> allMatches)
    {
        List<int> fallXIndex = new List<int>();

        foreach(Gem gem in allMatches)
        {
            if (gem == null)
            {
                continue;
            }
            gem.transform.GetComponent<SpriteRenderer>().enabled = false;

            disappearedGems.Add(gem);

            //Debug.Log("Disappear at "+gem.xIndex+" , "+gem.yIndex);
            gems[gem.xIndex, gem.yIndex] = null;

            int xFallIndex = gem.xIndex;
            if (!fallXIndex.Contains(xFallIndex))
            {
                fallXIndex.Add(xFallIndex);
            }
        }

        gm.ChangeScore(allMatches.Count);

        return fallXIndex;
    }

    List<Gem> FallGems(List<int> fallColumn)
    {
        List<Gem> movedGems = new List<Gem>();
        foreach (int fll in fallColumn)
        {
            movedGems = CombineLinks(movedGems, FallColumn(fll));
        }
        return movedGems;
    }

    List<Gem> FallColumn(int x)
    {
        List<Gem> movedGems = new List<Gem>();
        for (int y = 0; y < boardHeigth - 1; y++)
        {
            if (gems[x, y] == null)
            {
                //Debug.Log(x+" , "+y+"NotFound");
                for (int i = y+1; i < boardHeigth; i++)
                {
                    if(gems[x,i] != null)
                    {
                        Gem moveGem = gems[x, i];
                        movedGems.Add(moveGem);

                        moveGem.Fall(y);
                        

                        maxFallDistance = (maxFallDistance > (i - y)) ? maxFallDistance : (i - y);

                        break;
                    }
                }
            }
        }
        return movedGems;
    }

    #endregion

    bool IsInBoard(int x,int y)
    {
        if (x < 0 || x > boardWitdh - 1 || y < 0 || y > boardHeigth - 1)
        {
            //Debug.Log("Out Of Border");
            return false;
        }
        return true;
    }

    List<Gem> CombineLinks(List<Gem> list1 , List<Gem> list2)
    {
        if(list1 == null && list2 == null)
        {
            return new List<Gem>();
        }
        else if(list1 == null)
        {
            return list2;
        }
        else if(list2 == null)
        {
            return list1;
        }

        foreach (Gem gem in list1)
        {
            if (!list2.Contains(gem))
            {
                list2.Add(gem);
            }
        }

        return list2;
    }
    List<int> CombineLinks(List<int> list1, List<int> list2)
    {
        if (list1 == null && list2 == null)
        {
            return new List<int>();
        }
        else if (list1 == null)
        {
            return list2;
        }
        else if (list2 == null)
        {
            return list1;
        }

        foreach (int xIndex in list2)
        {
            if (!list1.Contains(xIndex))
            {
                list1.Add(xIndex);
            }
        }

        return list1;
    }

}
