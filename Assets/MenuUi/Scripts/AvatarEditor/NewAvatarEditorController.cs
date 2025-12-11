using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.AvatarEditor;
using UnityEngine;

namespace MenuUi.scripts.AvatarEditor
{
    public class NewAvatarEditorController : MonoBehaviour
    {
        //Planning to add this to the original AvatarEditorController, but find it easier to do in it's own script while making it
        [SerializeField] ScrollBarCategoryLoader _categoryLoader;
        [SerializeField] ScrollBarFeatureLoader _featureLoader;
        [SerializeField] ColorGridLoader _colorLoader;

        //disabling for now
        [SerializeField] private GameObject _editorMenu;
        [SerializeField] private GameObject _switchModeButtons;
        [SerializeField] private GameObject _revertButton;


        // Start is called before the first frame update
        void Start()
        {
            _categoryLoader.SetCategoryCells((categoryId) => _featureLoader.RefreshFeatureListItems(categoryId));
            _categoryLoader.UpdateCellSize();
            _colorLoader.SetColorCells();
            _colorLoader.UpdateCellSize();

            StartCoroutine(Wait());
        }

        //This is stupid but i don't want to remove the old objects yet so it works for now to clean up the view
        private IEnumerator Wait()
        {
            yield return new WaitForSeconds(1);
            _switchModeButtons.SetActive(false);
            _editorMenu.SetActive(false);
            _revertButton.SetActive(false);
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
