using GameUtil;

namespace SnakeGame;

public partial class MainPage : ContentPage
{
    GameController gameController = new();
    private bool connected;

    public MainPage()
    {
        InitializeComponent();
        
        worldPanel.SetWorld(gameController.GetWorld());
        gameController.UpdateArrived += OnFrame;
        gameController.Error += ShowError;
        gameController.Connected += UpdateConnect;
        
    }

    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }
    private void UpdateConnect()
    {
        connected = true;
    }

    void OnTextChanged(object sender, TextChangedEventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();
        if (text == "w")
        {
            gameController.MovePlayer("up");
        }
        else if (text == "a")
        {
            gameController.MovePlayer("left");
        }
        else if (text == "s")
        {
            gameController.MovePlayer("down");
        }
        else if (text == "d")
        {
            gameController.MovePlayer("right");
        }
        entry.Text = "";
    }

    private void NetworkErrorHandler()
    {
        DisplayAlert("Error", "Disconnected from server", "OK");
    }


    /// <summary>
    /// Event handler for the connect button
    /// We will put the connection attempt interface here in the view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ConnectClick(object sender, EventArgs args)
    {
        if (serverText.Text == "")
        {
            DisplayAlert("Error", "Please enter a server address", "OK");
            return;
        }
        if (nameText.Text == "")
        {
            DisplayAlert("Error", "Please enter a name", "OK");
            return;
        }
        if (nameText.Text.Length > 16)
        {
            DisplayAlert("Error", "Name must be less than 16 characters", "OK");
            return;
        }
        gameController.Connect(serverText.Text, nameText.Text);
        connectButton.IsEnabled = false;
        keyboardHack.Focus();
    }

    /// <summary>
    /// Use this method as an event handler for when the controller has updated the world
    /// </summary>
    public void OnFrame()
    {
        Dispatcher.Dispatch(() => graphicsView.Invalidate());
    }

    private void ControlsButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Controls",
                     "W:\t\t Move up\n" +
                     "A:\t\t Move left\n" +
                     "S:\t\t Move down\n" +
                     "D:\t\t Move right\n",
                     "OK");
    }

    private void AboutButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("About",
      "SnakeGame solution\nArtwork by Jolie Uk and Alex Smith\nGame design by Daniel Kopta and Travis Martin\n" +
      "Implementation by ...\n" +
        "CS 3500 Fall 2022, University of Utah", "OK");
    }

    private void ShowError(string err)
    {
        Dispatcher.Dispatch(() =>
        {
            DisplayAlert("Error", err, "OK");
            connectButton.IsEnabled = true;
        });
        
    }

    private void ContentPage_Focused(object sender, FocusEventArgs e)
    {
        if (connected)
        {
            keyboardHack.Focus();
        } else
        {
            connectButton.IsEnabled = true;
        }
            
    }
}