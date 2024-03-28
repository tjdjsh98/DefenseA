﻿using System.Collections.Generic;

public interface IShop
{

    public List<ShopItem> ShopItemList { set; get; }
    public int RestockCost { set; get; }
    public void RestockShopItems();
}