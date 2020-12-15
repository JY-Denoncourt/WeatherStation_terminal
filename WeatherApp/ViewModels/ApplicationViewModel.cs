using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using WeatherApp.Commands;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        ///TODO 10 : Completed
        ///Ajouter Importer et exporter au menu d<applicationview

        #region Membres

        private BaseViewModel currentViewModel;
        private List<BaseViewModel> viewModels;
        private TemperatureViewModel tvm;
        private OpenWeatherService ows;
        private string filename;
        private string openFilename;
        private string saveFilename;
        private string fileContent;

        private VistaSaveFileDialog saveFileDialog;
        private VistaOpenFileDialog openFileDialog;

        #endregion



        #region Propriétés
        /// <summary>
        /// Model actuellement affiché
        /// </summary>
        public BaseViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set { 
                currentViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// String contenant le nom du fichier
        /// </summary>
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }

        public string OpenFilename
        {
            get
            {
                return openFilename;
            }
            set
            {
                openFilename = value;
            }
        }

        public string SaveFilename
        {
            get
            {
                return saveFilename;
            }
            set
            {
                saveFilename = value;
            }
        }

        public string FileContent
        {
            get { return fileContent; }
            set
            {
                fileContent = value;
                OnPropertyChanged();
            }
        }
        
        
        /// <summary>
        /// Commande pour changer la page à afficher
        /// </summary>
        public DelegateCommand<string> ChangePageCommand { get; set; }
        public DelegateCommand<string> ImportCommand { get; set; }
        public DelegateCommand<string> ExportCommand { get; set; }
        public DelegateCommand<string> ChangeLanguageCommand { get; set; }
        
        /// <summary>
        /// TODO 02 : Completed
        /// Ajouter ImportCommand
        /// </summary>

        /// <summary>
        /// TODO 02 : Completed
        /// Ajouter ExportCommand
        /// </summary>

        /// <summary>
        /// TODO 13a : Completed
        /// Ajouter ChangeLanguageCommand
        /// </summary>


        public List<BaseViewModel> ViewModels
        {
            get {
                if (viewModels == null)
                    viewModels = new List<BaseViewModel>();
                return viewModels; 
            }
        }

        #endregion



        #region Constructeur
        public ApplicationViewModel()
        {
            ChangePageCommand = new DelegateCommand<string>(ChangePage);
            ImportCommand = new DelegateCommand<string>(Import);
            ExportCommand = new DelegateCommand<string>(Export, CanExport);
            ChangeLanguageCommand = new DelegateCommand<string>(ChangeLanguage);
            
            /// TODO 03 : Completed
            /// Instancier ImportCommand qui doit appeler la méthode Import

            /// TODO 06 : Completed
            /// Instancier ExportCommand qui doit appeler la méthode Export
            /// Ne peut s'exécuter que la méthode CanExport retourne vrai

            /// TODO 13b : Completed
            /// Instancier ChangeLanguageCommand qui doit appeler la méthode ChangeLanguage

            initViewModels();          

            CurrentViewModel = ViewModels[0];

        }
        #endregion



        #region Méthodes Base
        void initViewModels()
        {
            /// TemperatureViewModel setup
            tvm = new TemperatureViewModel();

            string apiKey = "";

            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "DEVELOPMENT")
            {
                apiKey = AppConfiguration.GetValue("OWApiKey");
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.apiKey) && apiKey == "")
            {
                tvm.RawText = "Aucune clé API, veuillez la configurer";
            } else
            {
                if (apiKey == "")
                    apiKey = Properties.Settings.Default.apiKey;

                ows = new OpenWeatherService(apiKey);
            }
                
            tvm.SetTemperatureService(ows);
            ViewModels.Add(tvm);

            var cvm = new ConfigurationViewModel();
            ViewModels.Add(cvm);
        }


        private void ChangePage(string pageName)
        {            
            if (CurrentViewModel is ConfigurationViewModel)
            {
                ows.SetApiKey(Properties.Settings.Default.apiKey);

                var vm = (TemperatureViewModel)ViewModels.FirstOrDefault(x => x.Name == typeof(TemperatureViewModel).Name);
                if (vm.TemperatureService == null)
                    vm.SetTemperatureService(ows);                
            }

            CurrentViewModel = ViewModels.FirstOrDefault(x => x.Name == pageName);  
        }

        #endregion



        #region Methodes File
        /// <summary>
        /// Méthode qui exécute l'exportation
        /// </summary>
        /// <param name="obj"></param>


        private void saveToFile()
        {
            var resultat = JsonConvert.SerializeObject(tvm.Temperatures, Formatting.Indented);

            using (var tw = new StreamWriter(SaveFilename, false))
            {
                tw.WriteLine(resultat);
                tw.Close();
            }

            /// TODO 09 : Completed
            /// Code pour sauvegarder dans le fichier
            /// Voir 
            /// Solution : 14_pratique_examen
            /// Projet : serialization_object
            /// Méthode : serialize_array()
            /// 
            /// ---
            /// Algo
            /// Initilisation du StreamWriter
            /// Sérialiser la collection de températures
            /// Écrire dans le fichier
            /// Fermer le fichier           

        }


        private void openFromFile()
        {
            using (var sr = new StreamReader(OpenFilename))
            {
                FileContent += sr.ReadToEnd();

                if (FileContent != "")
                {
                    tvm.Temperatures = JsonConvert.DeserializeObject<ObservableCollection<TemperatureModel>>(FileContent);
                    FileContent = "";
                }
            }


            /// TODO 05 : Completed
            /// Code pour lire le contenu du fichier
            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : serialization_object
            /// Méthode : deserialize_from_file_to_object
            /// 
            /// ---
            /// Algo
            /// Initilisation du StreamReader
            /// Lire le contenu du fichier
            /// Désérialiser dans un liste de TemperatureModel
            /// Remplacer le contenu de la collection de Temperatures avec la nouvelle liste

        }

        #endregion



        #region Methodes Import - Export 
        private void Import(string obj)
        {
            if (openFileDialog == null)
            {
                openFileDialog = new VistaOpenFileDialog();
                openFileDialog.Filter = "Json file|*.json|All files|*.*";
                openFileDialog.DefaultExt = "json";
            }

            if (openFileDialog.ShowDialog() == true)
            {
                OpenFilename = openFileDialog.FileName;
                openFromFile();
            }

            /// TODO 04 : Completed
            /// Commande d'importation : Code pour afficher la boîte de dialogue
            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : demo_openFolderDialog
            /// ---
            /// Algo
            /// Si la réponse de la boîte de dialogue est vraie
            ///   Garder le nom du fichier dans Filename
            ///   Appeler la méthode openFromFile

        }


        private void Export(string obj)
        {

            if (saveFileDialog == null)
            {
                saveFileDialog = new VistaSaveFileDialog();
                saveFileDialog.Filter = "Json file|*.json|All files|*.*";
                saveFileDialog.DefaultExt = "json";
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFilename = saveFileDialog.FileName;
                saveToFile();
            }

            /// TODO 08 : Completed
            /// Code pour afficher la boîte de dialogue de sauvegarde
            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : demo_openFolderDialog
            /// ---
            /// Algo
            /// Si la réponse de la boîte de dialogue est vrai
            ///   Garder le nom du fichier dans Filename
            ///   Appeler la méthode saveToFile
            ///   

        }


        private bool CanExport(string obj)
        {
            if (tvm.Temperatures != null) return true;
            else return false;

            /// <summary>
            /// TODO 07 : Completed
            /// Méthode CanExport ne retourne vrai que si la collection a du contenu
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            /// 
        }
        #endregion



        #region Internationalisation
        private void ChangeLanguage (string language)
        {
            /// TODO 13c : Compléter la méthode pour permettre de changer la langue
            /// Ne pas oublier de demander à l'utilisateur de redémarrer l'application
            /// Aide : ApiConsumerDemo
        }

        #endregion
    }
}
