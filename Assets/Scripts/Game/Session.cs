﻿using UnityEngine;

public class Session
{
    public void Update( bool addConveyorItems )
    {
        if ( running)
        {
            coinCounter.SetCounterValue(player.inventory.coins);

            //While there are strictly speaking better ways to get a world-space mouse position, this one has the absolute minimum number of moving parts
            //First we get a ray. The camera has a convenience method that returns a ray from the center of the camera in the direction of a screen point
            //The mouse position we can poll via the input system is in screen-space coordinates
            Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);

            //The actual raycast returns an array with all the targets the ray passed through
            //Note that we don't pass in the ray itself -- that's because the method taking a ray as argument flat-out doesn't work
            //We don't bother constraining the raycast by layer mask just yet, since the ground plane is the only collider in the scene
            RaycastHit[] hits = Physics.RaycastAll(mouseRay.origin, mouseRay.direction, float.PositiveInfinity);

            //These references might be populated later
            Lane hoveredLane = null;
            ConveyorItem hoveredItem = null;

            //Proceed if raycast hits something
            if (hits.Length > 0)
            {
                //Get the mouse position on the ground plane
                Vector3 mousePosition = hits[ 0 ].point;

                //See if the mouse is hovering any lanes
                hoveredLane = stage.GetHoveredLane(mousePosition);

                //Proceed if the mouse is hovering the conveyor
                if (conveyor.Contains(mousePosition))
                {
                    //Try to get a hovered conveyor item
                    hoveredItem = conveyor.GetHoveredItem(mousePosition);

                    //Proceed if an item is hovered and no item is held
                    if (hoveredItem != null && heldItem == null)
                    {
                        //Instantiate a new HeldItem if no item is held and the left mouse button is pressed
                        //Otherwise, change the color of the item to indicate hover
                        if (heldItem == null && Input.GetMouseButtonDown(0))
                            heldItem = new HeldItem(hoveredItem);
                        else
                            hoveredItem.color = Color.yellow;
                    }
                }

                //Reset lane colors
                stage.SetLaneColor(Color.black);

                if (heldItem != null) //Proceed if an item is held
                {
                    //Position the held item at the world-space mouse position
                    heldItem.SetPosition(mousePosition);

                    //Proceed if a lane is hovered
                    if (hoveredLane != null)
                        hoveredLane.color = Color.yellow;

                    //Proceed if the left mouse button is released
                    if (!Input.GetMouseButton(0))
                    {
                        hoveredItem = conveyor.GetHoveredItem(mousePosition);

                        if (hoveredLane != null)
                        {
                            hoveredLane.Add(new LaneItem(heldItem, hoveredLane));
                            heldItem.Destroy();
                        }

                        if (hoveredItem != null && heldItem.conveyorItem != hoveredItem)
                        {
                            ItemDefinition heldDefinition = heldItem.conveyorItem.definition;
                            ItemSettings heldSettings = heldItem.conveyorItem.settings;
                            ItemDefinition hoveredDefinition = hoveredItem.definition;
                            ItemSettings hoveredSettings = hoveredItem.settings;

                            heldItem.conveyorItem.SetItemDefinition(hoveredDefinition);
                            heldItem.conveyorItem.SetItemSettings(hoveredSettings);
                            hoveredItem.SetItemDefinition(heldDefinition);
                            hoveredItem.SetItemSettings(heldSettings);
                            heldItem.conveyorItem.Refresh();
                            hoveredItem.Refresh();
                        }

                        //Reset the held conveyor item's color and clean up the held item
                        heldItem.conveyorItem.color = Color.white;
                        heldItem.conveyorItem.SetHeld(false);
                        heldItem.Destroy();
                        heldItem = null;
                    }
                }
            }

            //Reset the color of any item not currently hovered
            conveyor.SetItemColor(Color.white, heldItem != null ? heldItem.conveyorItem : hoveredItem);

            //Update the conveyor
            conveyor.Update();

            //Update the stage
            stage.Update();

            //Update the level
            level.Update();

            //Proceed if the item spawn interval has elapsed, and add a new item to the conveyor belt
            if (Time.time > itemTime && addConveyorItems)
                itemTime = conveyor.AddItemToConveyor(player.inventory);
        }
    }

    public void Destroy()
    {
        stage?.Destroy();
        coinCounter?.Destroy();
        level?.DestroyProgress();
    }

    public void Show()
    {
        level?.ShowProgress();
        coinCounter?.Show();
        stage?.ShowLanes();
        conveyor?.Show();
    }

    public void Hide()
    {
        level?.HideProgress();
        coinCounter?.Hide();
        stage?.HideLanes();
        conveyor?.Hide();
    }

    public void Start() => running = true;
    public void Stop() => running = false;

    public void SetStage(Stage stage) => SetConveyor((this.stage = stage).conveyor);
    public void SetConveyor(Conveyor conveyor) => this.conveyor = conveyor;
    public void SetLevel(Level level) => this.level = level;

    public Player player { get; }
    public CoinCounter coinCounter { get; }
    public Level level { get; private set; }
    public Stage stage { get; private set; }
    public bool running { get; private set; }
    public Conveyor conveyor { get; private set; }
    public HeldItem heldItem { get; private set; }

    private Camera camera => Camera.main;
    private float itemTime { get; set; }

    public Session( Player player )
    {
        this.player = player;
        coinCounter = new CoinCounter();
    }
}
