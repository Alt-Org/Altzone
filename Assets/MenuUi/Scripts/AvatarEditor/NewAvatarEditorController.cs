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

        //disabling for now
        [SerializeField] private GameObject _editorMenu;
        [SerializeField] private GameObject _switchModeButtons;
        [SerializeField] private GameObject _revertButton;


        // Start is called before the first frame update
        void Start()
        {
            _categoryLoader.SetCategoryCells((categoryId) => _featureLoader.RefreshFeatureListItems(categoryId));
            _categoryLoader.UpdateCellSize();

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
