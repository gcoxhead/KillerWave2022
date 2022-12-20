using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class PlayerShipBuild_OLD : MonoBehaviour
{
	[SerializeField]
	GameObject[] shopButtons;
	GameObject target;
	GameObject tmpSelection;
	GameObject textBoxPanel;
	[SerializeField]
	GameObject[] visualWeapons;
	[SerializeField]
	SOActorModel defaultPlayerShip;
	GameObject playerShip;
	public GameObject buyButton;
	GameObject bankObj;
	int bank = 600;
	bool purchaseMade = false;
	[SerializeField] string androidGameId;
	[SerializeField] string iOSGameId;
	[SerializeField] bool testMode = true;
	string adId = null;

	void Awake()
	{
		CheckPlatform();
	}

	void Start()
	{
		//StartCoroutine(WaitForAd());
		TurnOffSelectionHighlights();
		textBoxPanel = GameObject.Find("textBoxPanel");
		purchaseMade = false;
		bankObj = GameObject.Find("bank");
		bankObj.GetComponentInChildren<TextMesh>().text = bank.ToString();
		buyButton = textBoxPanel.transform.Find("BUY ?").gameObject;

		TurnOffPlayerShipVisuals();
		PreparePlayerShipForUpgrade();
	}


	void CheckPlatform()
	{
		string gameId = null;
#if UNITY_IOS
	{
		gameId = iOSGameId;
		adId = "Rewarded_iOS";
	}
#elif UNITY_ANDROID
	{
		gameId = androidGameId;
		adId = "Rewarded_Android";
	}
#endif
		
	}

	void TurnOffSelectionHighlights()
	{
		for (int i = 0; i < shopButtons.Length; i++)
		{
			shopButtons[i].SetActive(false);
		}
	}
	void Update()
	{
		AttemptSelection();
	}
	GameObject ReturnClickedObject(out RaycastHit hit)
	{
		GameObject target = null;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if (Physics.Raycast(ray.origin, ray.direction * 100, out hit))
		{
			target = hit.collider.gameObject;
		}
		return target;
	}
	void AttemptSelection()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hitInfo;
			target = ReturnClickedObject(out hitInfo);
			if (target != null)
			{
				if (target.transform.Find("itemText"))
				{
					TurnOffSelectionHighlights();
					Select();
					UpdateDescriptionBox();

					//NOT ALREADY SOLD
					if (target.transform.Find("itemText").GetComponent<TextMesh>().text != "SOLD")
					{
						//can afford
						Affordable();
						//can not afford
						LackOfCredits();
					}
					else if (target.transform.Find("itemText").GetComponent<TextMesh>().text == "SOLD")
					{
						SoldOut();
					}
				}
				else if (target.name == "BUY ?")
				{
					BuyItem();
				}
				else if (target.name == "START")
				{
					StartGame();
				}
				else if (target.name == "WATCH AD")
				{
					
				}
			}
		}
	}
	

	void StartGame()
	{
		if (purchaseMade)
		{
			playerShip.name = "UpgradedShip";
			if (playerShip.transform.Find("energy +1(Clone)"))
			{
				playerShip.GetComponent<Player>().Health = 2;
			}
			DontDestroyOnLoad(playerShip);
		}
		GameManager.Instance.GetComponent<ScenesManager>().BeginGame(GameManager.gameLevelScene);
	}

	void BuyItem()
	{
		Debug.Log("PURCHASED");
		purchaseMade = true;
		buyButton.SetActive(false);
		tmpSelection.SetActive(false);

		for (int i = 0; i < visualWeapons.Length; i++)
		{
			if (visualWeapons[i].name ==
			tmpSelection.transform.parent.gameObject.GetComponent<ShopPiece>().ShopSelection.iconName)
			{
				visualWeapons[i].SetActive(true);
			}
		}

		UpgradeToShip(tmpSelection.transform.parent.gameObject.GetComponent<ShopPiece>().ShopSelection.iconName);
		bank = bank - System.Int32.Parse(tmpSelection.transform.parent.GetComponent<ShopPiece>().ShopSelection.cost);
		bankObj.transform.Find("bankText").GetComponent<TextMesh>().text = bank.ToString();
		tmpSelection.transform.parent.transform.Find("itemText").GetComponent<TextMesh>().text = "SOLD";
	}

	void UpgradeToShip(string upgrade)
	{
		GameObject shipItem = GameObject.Instantiate(Resources.Load(upgrade)) as GameObject;
		shipItem.transform.SetParent(playerShip.transform);
		shipItem.transform.localPosition = Vector3.zero;
	}

	void Select()
	{
		tmpSelection = target.transform.Find("SelectionQuad").gameObject;
		tmpSelection.SetActive(true);
	}

	void UpdateDescriptionBox()
	{
		textBoxPanel.transform.Find("name").gameObject.GetComponent<TextMesh>().text = tmpSelection.GetComponentInParent<ShopPiece>().ShopSelection.iconName;
		textBoxPanel.transform.Find("desc").gameObject.GetComponent<TextMesh>().text = tmpSelection.GetComponentInParent<ShopPiece>().ShopSelection.description;
	}


	void Affordable()
	{
		if (bank >= System.Int32.Parse(target.transform.GetComponent<ShopPiece>().ShopSelection.cost))
		{
			Debug.Log("CAN BUY");
			buyButton.SetActive(true);
		}
	}
	void LackOfCredits()
	{
		if (bank < System.Int32.Parse(target.transform.Find("itemText").GetComponent<TextMesh>().text))
		{
			Debug.Log("CAN'T BUY");
		}
	}
	void SoldOut()
	{
		Debug.Log("SOLD OUT");
	}
	void TurnOffPlayerShipVisuals()
	{
		for (int i = 0; i < visualWeapons.Length; i++)
		{
			visualWeapons[i].gameObject.SetActive(false);
		}
	}
	void PreparePlayerShipForUpgrade()
	{
		playerShip = GameObject.Instantiate(defaultPlayerShip.actor);
		playerShip.GetComponent<Player>().enabled = false;
		playerShip.transform.position = new Vector3(0, 10000, 0);
		playerShip.GetComponent<IActorTemplate>().ActorStats(defaultPlayerShip);
	}

}
