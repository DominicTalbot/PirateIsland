using System;

[Serializable]
public class CargoStack
{
    public CargoType type;

    public int amount;

    public CargoStack(
        CargoType type,
        int amount
    )
    {
        this.type = type;
        this.amount = amount;
    }
}