using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance;

    public List<ShipState> ships =
        new List<ShipState>();

    private void Awake()
    {
        if (
            Instance != null &&
            Instance != this
        )
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Debug.Log(
    "SHIP MANAGER AWAKE | Instance ID: " +
    GetInstanceID()
);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ShipState ship =
            GetShip("ship_001");

        if (ship == null)
        {
            ship =
                CreateShip(
                    "ship_001",
                    "Starter Ship"
                );
        }

        Debug.Log(
            "SHIP MANAGER READY | " +
            ship.shipName +
            " | Supplies: " +
            ship.supplies +
            " | Cargo Level: " +
            ship.cargoLevel +
            " | On Voyage: " +
            ship.onVoyage
        );
    }

    public ShipState GetShip(
        string shipId
    )
    {
        foreach (
            ShipState ship
            in ships
        )
        {
            if (
                ship.shipId ==
                shipId
            )
            {
                return ship;
            }
        }

        return null;
    }

    public ShipState CreateShip(
        string shipId,
        string shipName
    )
    {
        ShipState ship =
            GetShip(shipId);

        if (ship != null)
        {
            return ship;
        }

        ship =
    new ShipState();

        ship.shipId =
            shipId;

        ship.shipName =
            shipName;


        // =========================================================
        // NEW SHIP DEFAULTS
        // =========================================================

        ship.supplies = 10;

        ship.cargoCapacity = 20;

        ship.morale = 100f;

        ship.crewCount = 0;

        ship.sailLevel = 1;

        ship.cargoLevel = 1;

        ship.cannonLevel = 1;


        ships.Add(ship);

        Debug.Log(
            "SHIP CREATED: " +
            ship.shipName
        );

        return ship;
    }
}