using KModkit;
using System;
using UnityEngine;
using Rnd = UnityEngine.Random;
using wawa.DDL;

public class ParlorBox : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Bomb;

    public TextMesh[] BoxText;
    public MeshRenderer[] BoxMeshes;
    public MeshRenderer[] BoxLids;
    public Material[] BoxMaterials;

    static int moduleIdCounter = 1;
    int moduleId;

    int ducksMentioned, gemsMentioned, fillerCount, freebieCount, trueBox,falseBox,duckLocation,gemLocation;
    private bool[][] truthArray = new bool[3][]
    {
        new bool[3],new bool[3], new bool[3]
    };
    private int[][] puzzleTextIDs = new int[3][]
    {
        new int[3],
        new int[3],
        new int[3]
    };
    private string[] boxNames = new string[3];

    private static string[] boxNamePossibilites =
    {
        "Alpha", "Bravo","Charles","Delta", "Epsilon","Foxy", "Geist","Hank","Indiana","Jake"
    };

    private static string[] statementText = new string[]
    {
        //freebies
        "There is a\n\"{0}\"\non the bomb",
        "This box's name is\n\"{0}\"",
        "This box is the color\n{0}",
        "This box is on\nthe {0}",
        "The other two boxes\ncontain {0}",
        "The {0} box\nis named\n\"{1}\"",
        "There are three boxes\non the module",
        "There are {0}\nstatements\non this box",
        //not freebies
        "The box that claims\nto be {0}\ncontains {1}",
        "Another statement\non this box\nis {0}",
        "Another visible\nstatement is {0}",
        "The {0} box's\nstatement is\n{1}",
        "Another box contains\n{0}",
        "The {0} box\n contains {1}",
        "The other two boxes\ncontain the gems\nand the duck",
        "A box with a {0}\nstatement contains\n{1}",
        "A box showing a\n{0} statement\n contains {1}",
        "You will open this box\nand find {0}",
        "Only one visible\nstatement is {0}",
        "Only one other\nvisible statement\nis {0}, and it\ncontains {1}",
        "The {0} are in\nthe box that's\nall {1}",
        "The {0} are in\nthe box that's\nshowing the word\n\"{1}\"",
        "The {0} are in\nthe box that's\nshowing the letter\n\"{1}\"",
        "The {0} are in\nthe box who's\nname contains the letter\n\"{1}\"",
        "Other statements\nwith the word\n\"{0}\"\nare always {1}",
        "The {0} are in\nthe box with the only\nvisible {1} statement",
        "This box contains\n{0}",
        "There are {0}\n{1} statements",
        "Other statements with\n{0} numbers of words\nare always {1}",
        "That's {0}",
        "This box's statement\nwhen the key is in the\n{0} box is {1}",
        "The {0} box has\n{1} {2} statements",
        "There is not a box\nwith a mix of true\nand false statements",
        "Exactly {0} of the\n{1} box's statements\nare {2}",
        "There is no box that\ndisplays {0} {1}\nstatements",
        //filler
        "This statement is\nof no help at all" //can actually be helpful, notably
    };
    private static string[] allowedWords = new string[]
    {
        "box", "gems", "duck", "true","false","name","visible","one","two","three","statement","top","left","right"
    };
    private static string[] numberWords = new string[]
    {
        "zero", "one", "two","three","four","five","six","seven","eight","nine"
    };
    private static string[] placementWords = new string[]
    {
        "top","left","right"
    };

    void Awake()
    {
        moduleId = moduleIdCounter++;
    }
    // Use this for initialization
    void Start () {
		for(int i =0;i<3;i++)
        {
            BoxText[i].text = statementText[Rnd.Range(0, statementText.Length)];
        }
        GeneratePuzzle();
	}

    void GeneratePuzzle()
    {
        trueBox = Rnd.Range(0, 3);
        gemLocation = Rnd.Range(0, 3);
        do
        {
            falseBox = Rnd.Range(0, 3);
            duckLocation = Rnd.Range(0, 3);
        } while (falseBox == trueBox || gemLocation == duckLocation);
        Debug.LogFormat("[Parlor Box #{0}]: The gems are in Box #{1}. The duck is in Box #{2}", moduleId, gemLocation+1, duckLocation+1);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (i == trueBox)
                    truthArray[i][j] = true;
                else if (i == falseBox)
                    truthArray[i][j] = false;
                else
                    truthArray[i][j] = Rnd.Range(0, 2) == 1;
            }
        }
        Debug.LogFormat("[Parlor Box #{0}]: The statements' truth is as follows: {1}; {2}; {3}", moduleId, truthArray[0].Join(", "), truthArray[1].Join(", "), truthArray[2].Join(", "));
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
