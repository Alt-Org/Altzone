using UnityEngine;

public class PopupWindowScript : MonoBehaviour
{
    // Muuttuja sen taskin id:n tallentamiseksi, jota t�m� popup-ikkuna hallinnoi
    public int associatedTaskId;

    // Metodi avaa popup-ikkunan
    public void OpenPopupWindow()
    {
        // Tarkista, onko taskin id asetettu
        if (associatedTaskId <= 0)
        {
            Debug.LogWarning("Popup-ikkunan avaus ep�onnistui: Taskin id ei ole asetettu.");
            return;
        }

        // T�ss� voisi olla koodi popup-ikkunan avaamiseksi
        Debug.Log("Popup-ikkuna avattu taskille: " + associatedTaskId);
    }

    // Metodi sulkee popup-ikkunan
    public void ClosePopupWindow()
    {
        // T�ss� voisi olla koodi popup-ikkunan sulkemiseksi
        Debug.Log("Popup-ikkuna suljettu");
    }

    // Metodi asettaa sen taskin id:n, jota t�m� popup-ikkuna hallinnoi
    public void SetAssociatedTaskId(int taskId)
    {
        // Tarkista, ett� taskin id on kelvollinen
        if (taskId <= 0)
        {
            Debug.LogWarning("Virhe: Virheellinen taskin id. Taskin id:n on oltava positiivinen kokonaisluku.");
            return;
        }

        // Aseta taskin id
        associatedTaskId = taskId;
        Debug.Log("Popup-ikkunalle asetettu sen taskin id: " + associatedTaskId);
    }

    // Metodi nollaa sen taskin id:n, jota t�m� popup-ikkuna hallinnoi
    public void ResetAssociatedTaskId()
    {
        // Nollaa taskin id
        associatedTaskId = 0;
        Debug.Log("Popup-ikkunan hallinnoima taskin id nollattu");
    }
}
