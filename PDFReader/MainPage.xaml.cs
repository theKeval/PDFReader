using MuPDFWinRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace PDFReader
{
    public sealed partial class MainPage
    {
        DocumentPage currentPage;
        Document pdfDocument;
        ScrollViewer scrollViewer;
        readonly List<DocumentPage> pages = new List<DocumentPage>();

        public static int pageCount = 0;
        public static int currentPageNumber;
        public static bool isMoveForward;


        //public MainPage()
        //{
        //    this.InitializeComponent();

        //    InkMode();

        //    InkCanvas.PointerPressed += new PointerEventHandler(OnCanvasPointerPressed);
        //    InkCanvas.PointerMoved += new PointerEventHandler(OnCanvasPointerMoved);
        //    InkCanvas.PointerReleased += new PointerEventHandler(OnCanvasPointerReleased);
        //    InkCanvas.PointerExited += new PointerEventHandler(OnCanvasPointerReleased);
        //}

        //public MainPage()
        //{
        //    InitializeComponent();

        //    InkMode();

        //    InkCanvas.PointerPressed += new PointerEventHandler(OnCanvasPointerPressed);
        //    InkCanvas.PointerMoved += new PointerEventHandler(OnCanvasPointerMoved);
        //    InkCanvas.PointerReleased += new PointerEventHandler(OnCanvasPointerReleased);
        //    InkCanvas.PointerExited += new PointerEventHandler(OnCanvasPointerReleased);
        //    InkCanvas.PointerCaptureLost += new PointerEventHandler(OnCanvasPointerReleased);
        //}


        public StorageFolder fileStorageFolder;

        private async void Page_Loaded_1(object sender, RoutedEventArgs e)
        {
            // Loading the document
            //var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"test.pdf");

            #region new code for open selected file

            //var file = await StorageFile.GetFileFromPathAsync(App.FilePath);

            try
            {

                FileOpenPicker fop = new FileOpenPicker();
                fop.FileTypeFilter.Add(".pdf");
                fop.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                fop.CommitButtonText = "Open PDF";

                var PDFfile = await fop.PickSingleFileAsync();
                if (PDFfile != null)
                {
                    App.FilePath = PDFfile.Path;
                    App.FileDisplayName = PDFfile.DisplayName;    // this name included with extension .pdf

                    fileStorageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(App.FileDisplayName, CreationCollisionOption.OpenIfExists);
                }

                using (var stream = await PDFfile.OpenReadAsync())
                {
                    IBuffer readBuffer;
                    using (IInputStream inputStreamAt = stream.GetInputStreamAt(0))
                    using (var dataReader = new DataReader(inputStreamAt))
                    {
                        uint bufferSize = await dataReader.LoadAsync((uint)stream.Size);
                        readBuffer = dataReader.ReadBuffer(bufferSize);
                    }

                    pdfDocument = Document.Create(readBuffer, DocumentType.PDF, (int)Windows.Graphics.Display.DisplayProperties.LogicalDpi);
                }

                if (pdfDocument.PageCount == 0)
                    return;

                for (var index = 0; index < pdfDocument.PageCount; index++)
                {
                    pages.Add(new DocumentPage(pdfDocument, index, ActualHeight));
                }

                // Initializing pageCount variable
                // pageCount = 0;

                flipView.SelectionChanged += flipView_SelectionChanged;
                flipView.Loaded += flipView_Loaded;
                flipView.ItemsSource = pages;

            }
            catch (NullReferenceException nre)
            {
                this.Frame.Navigate(typeof(HomePage));
            }
            catch (Exception ex)
            {
                MessageDialog ms = new MessageDialog(ex.Message);
                ms.ShowAsync();
            }

            

            #endregion

            #region commented for fixed file

            //var files = await (await Package.Current.InstalledLocation.GetFolderAsync("Assets")).GetFilesAsync();
            //foreach (var file in files)
            //{
            //    if (file.Name == "Introducing Windows Azure.png")
            //    {
            //        using (var stream = await file.OpenReadAsync())
            //        {
            //            IBuffer readBuffer;
            //            using (IInputStream inputStreamAt = stream.GetInputStreamAt(0))
            //            using (var dataReader = new DataReader(inputStreamAt))
            //            {
            //                uint bufferSize = await dataReader.LoadAsync((uint)stream.Size);
            //                readBuffer = dataReader.ReadBuffer(bufferSize);
            //            }

            //            pdfDocument = Document.Create(readBuffer, DocumentType.PDF, (int)Windows.Graphics.Display.DisplayProperties.LogicalDpi);
            //        }

            //        if (pdfDocument.PageCount == 0)
            //            return;

            //        for (var index = 0; index < pdfDocument.PageCount; index++)
            //        {
            //            pages.Add(new DocumentPage(pdfDocument, index, ActualHeight));
            //        }

            //        // Initializing pageCount variable
            //        // pageCount = 0;

            //        flipView.SelectionChanged += flipView_SelectionChanged;
            //        flipView.Loaded += flipView_Loaded;
            //        flipView.ItemsSource = pages;
            //    }
            //}

            #endregion

        }

        void flipView_Loaded(object sender, RoutedEventArgs e)
        {
            // pageCount = 0;

            OnSelectionChanged();
        }

        bool isFirstTime = true;
        public int counter = 0;

        public async void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flipView.SelectedIndex != -1)
            {
                var tempPageNumber = currentPageNumber;
                currentPageNumber = flipView.SelectedIndex;

                if (tempPageNumber < currentPageNumber)
                {
                    isMoveForward = true;
                }
                else
                {
                    isMoveForward = false;
                }
            }

            counter++;

            if (counter == 1 || counter == 2 || counter == 3)
            {
                #region if first page => read ink if there
                if (counter == 3)
                {
                    try
                    {
                        //var check = await DoesFileExist((flipView.SelectedIndex+1).ToString() + ".png");
                        var check = await DoesFileExist((flipView.SelectedIndex).ToString() + ".png");
                        if (check)
                        {
                            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(App.FileDisplayName);
                            StorageFile filesave = await storageFolder.GetFileAsync((flipView.SelectedIndex).ToString() + ".png");
                            ReadInk(filesave);
                        }
                        //else
                        //{

                        //}

                        var check_HighLight = await DoesFileExist((flipView.SelectedIndex).ToString() + "_HighLight.png");
                        if (check_HighLight)
                        {
                            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(App.FileDisplayName);
                            StorageFile filesave_HighLight = await storageFolder.GetFileAsync((flipView.SelectedIndex).ToString() + "_HighLight.png");
                            ReadInk_forHighLight(filesave_HighLight);
                        }
                        //else
                        //{

                        //}

                    }
                    catch (Exception ex)
                    {

                        var dlge = new MessageDialog(ex.Message);
                        dlge.ShowAsync();
                    }
                }
                #endregion
            }

            else
            {
                try
                {
                    var abc = m_InkManager.GetStrokes();
                    var abc_HighLight = m_HighLightManager.GetStrokes();

                    #region save on page change -> Pen Ink

                    if (abc.Count == 0)
                    {
                        //return;

                        StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(App.FileDisplayName, CreationCollisionOption.OpenIfExists);
                        if (isMoveForward)
                        {
                            if (await DoesFileExist((flipView.SelectedIndex - 1).ToString() + ".png"))
                            {
                                var InkFile = await storageFolder.GetFileAsync((flipView.SelectedIndex - 1).ToString() + ".png");
                                await InkFile.DeleteAsync();   
                            }
                        }
                        else
                        {
                            if (await DoesFileExist((flipView.SelectedIndex + 1).ToString() + ".png"))
                            {
                                var InkFile = await storageFolder.GetFileAsync((flipView.SelectedIndex + 1).ToString() + ".png");
                                await InkFile.DeleteAsync();
                            }
                        }
                    }
                    else
                    {
                        StorageFile fileToSave;
                        StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(App.FileDisplayName, CreationCollisionOption.OpenIfExists);
                        if (isMoveForward)
                        {
                            fileToSave = await storageFolder.CreateFileAsync((flipView.SelectedIndex - 1).ToString() + ".png", CreationCollisionOption.ReplaceExisting);
                        }
                        else
                        {
                            fileToSave = await storageFolder.CreateFileAsync((flipView.SelectedIndex + 1).ToString() + ".png", CreationCollisionOption.ReplaceExisting);
                        }

                        using (IOutputStream ab = await fileToSave.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            if (ab != null)
                                await m_InkManager.SaveAsync(ab);
                        }

                        // to delete all of the inkStrokes
                        var strokes = m_InkManager.GetStrokes();

                        for (int i = 0; i < strokes.Count; i++)
                        {
                            strokes[i].Selected = true;
                        }

                        m_InkManager.DeleteSelected();

                        RefreshCanvas();
                    }

                    #endregion

                    #region save on page change -> HighLight Ink

                    if (abc_HighLight.Count == 0)
                    {
                        // return;

                        StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(App.FileDisplayName, CreationCollisionOption.OpenIfExists);
                        if (isMoveForward)
                        {
                            if (await DoesFileExist((flipView.SelectedIndex - 1).ToString() + "_HighLight.png"))
                            {
                                var HighLightFile = await storageFolder.GetFileAsync((flipView.SelectedIndex - 1).ToString() + "_HighLight.png");
                                await HighLightFile.DeleteAsync();
                            }
                        }
                        else
                        {
                            if (await DoesFileExist((flipView.SelectedIndex + 1).ToString() + "_HighLight.png"))
                            {
                                var InkFile = await storageFolder.GetFileAsync((flipView.SelectedIndex + 1).ToString() + "_HighLight.png");
                                await InkFile.DeleteAsync();
                            }
                        }

                    }
                    else
                    {
                        StorageFile fileToSave_HighLight;
                        StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(App.FileDisplayName, CreationCollisionOption.OpenIfExists);
                        if (isMoveForward)
                        {
                            fileToSave_HighLight = await storageFolder.CreateFileAsync((flipView.SelectedIndex - 1).ToString() + "_HighLight.png", CreationCollisionOption.ReplaceExisting);
                        }
                        else
                        {
                            fileToSave_HighLight = await storageFolder.CreateFileAsync((flipView.SelectedIndex + 1).ToString() + "_HighLight.png", CreationCollisionOption.ReplaceExisting);
                        }

                        using (IOutputStream ab = await fileToSave_HighLight.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            if (ab != null)
                                await m_HighLightManager.SaveAsync(ab);
                        }

                        // to delete all of the inkStrokes
                        var strokes = m_HighLightManager.GetStrokes();

                        for (int i = 0; i < strokes.Count; i++)
                        {
                            strokes[i].Selected = true;
                        }

                        m_HighLightManager.DeleteSelected();

                        RefreshCanvas();
                    }

                    #endregion

                }
                catch (Exception ex)    // This Exception shows that the image with the same name already exist in localFolder
                {
                    var dlge = new MessageDialog(ex.Message);
                    dlge.ShowAsync();
                }


                var check = await DoesFileExist((flipView.SelectedIndex).ToString() + ".png");
                if (check)
                {
                    StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(App.FileDisplayName);
                    StorageFile filesave = await storageFolder.GetFileAsync((flipView.SelectedIndex).ToString() + ".png");
                    ReadInk(filesave);
                }
                else
                {

                }

                var check_forHighLight = await DoesFileExist((flipView.SelectedIndex).ToString() + "_HighLight.png");
                if (check_forHighLight)
                {
                    StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(App.FileDisplayName);
                    StorageFile fileSave = await storageFolder.GetFileAsync((flipView.SelectedIndex).ToString() + "_HighLight.png");
                    ReadInk_forHighLight(fileSave);
                    //RefreshCanvas();
                }
                else
                {

                }
            }

            #region commented

            //counter++;
            //if (counter == 1 || counter == 2 || counter == 3)
            //{
            //    if (counter == 3)
            //    {
            //        pageCount++;
            //        isFirstTime = false;

            //        #region commented
            //        //try
            //        //{
            //        //    Windows.Storage.Pickers.FileOpenPicker open = new Windows.Storage.Pickers.FileOpenPicker();
            //        //    open.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            //        //    open.FileTypeFilter.Add(".png");
            //        //    StorageFile filesave = await open.PickSingleFileAsync();

            //        //    //StorageFile filesave = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("2.png");

            //        //    ReadInk(filesave);
            //        //}
            //        //catch (Exception ex)
            //        //{

            //        //    var dlge = new MessageDialog(ex.Message);
            //        //    dlge.ShowAsync();
            //        //}
            //        #endregion

            //    }
            //    //pageCount++;
            //    //isFirstTime = false;
            //}
            //else
            //{
            //    pageCount++;

            //    #region save on page change

            //    try
            //    {
            //        var abc = m_InkManager.GetStrokes();
            //        if (abc.Count == 0)
            //        {
            //            return;
            //        }
            //        else
            //        {
            //            //var storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation.CreateFolderAsync("InkingImages", CreationCollisionOption.OpenIfExists);
            //            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("InkingImages", CreationCollisionOption.OpenIfExists);
            //            StorageFile fileToSave = await storageFolder.CreateFileAsync(pageCount.ToString() + ".png", CreationCollisionOption.GenerateUniqueName);

            //            //Windows.Storage.Pickers.FileSavePicker save = new Windows.Storage.Pickers.FileSavePicker();
            //            //save.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            //            //save.DefaultFileExtension = ".png";
            //            //save.FileTypeChoices.Add("PNG", new string[] { ".png" });
            //            //StorageFile filesave = await save.PickSaveFileAsync();
            //            using (IOutputStream ab = await fileToSave.OpenAsync(FileAccessMode.ReadWrite))
            //            {
            //                if (ab != null)
            //                    await m_InkManager.SaveAsync(ab);
            //            }

            //            // to delete all of the inkStrokes
            //            var strokes = m_InkManager.GetStrokes();

            //            for (int i = 0; i < strokes.Count; i++)
            //            {
            //                strokes[i].Selected = true;
            //            }

            //            m_InkManager.DeleteSelected();

            //            //InkCanvas.Children.Clear();
            //            //InkCanvas.Children.Add(flipView);

            //            RefreshCanvas();

            //            #region commented deleting strokes
            //            //var canvasChildrens = InkCanvas.Children;

            //            //foreach (var children in canvasChildrens)
            //            //{
            //            //    if (!(children is FlipView || children is Image))
            //            //    {
            //            //        var inkstrk = children;
            //            //        children.Visibility = Visibility.Collapsed;

            //            //        //InkCanvas.Children.Select<InkStroke, object>(a => { (InkStroke)strokes = Select(m_InkManager.DeleteSelected()); });
            //            //    }
            //            //}
            //            #endregion
            //        }

            //    }

            //    // This Exception shows that the image with the same name already exist in localFolder
            //    catch (Exception ex)
            //    {
            //        var dlge = new MessageDialog(ex.Message);
            //        dlge.ShowAsync();
            //    }

            //    #endregion

            //}

            #endregion

            OnSelectionChanged();
        }

        public async Task<bool> DoesFileExist(string filename)
        {
            bool flag = true;

            try
            {
                StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(App.FileDisplayName);
                var file = await storageFolder.GetFileAsync(filename);
            }
            catch (Exception ex)
            {
                if (ex.Message == "The system cannot find the file specified. (Exception from HRESULT: 0x80070002)")
                {
                    flag = false;
                }
            }

            return flag;
        }


        private void OnSelectionChanged()
        {
            if (scrollViewer != null)
            {
                scrollViewer.ViewChanged -= scrollViewer_ViewChanged;
            }

            if (flipView.SelectedIndex < 0)
            {
                return;
            }


            //var dataItem = flipView.ItemContainerGenerator.ContainerFromItem(flipView.SelectedItem);

            //if (dataItem == null)
            //{
            //    return;
            //}

            //scrollViewer = dataItem.GetDescendants().OfType<ScrollViewer>().FirstOrDefault();

            //if (scrollViewer != null)
            //{
            //    scrollViewer.ViewChanged += scrollViewer_ViewChanged;

            //    currentPage = (DocumentPage)flipView.SelectedItem;
            //    //pageCount++;
            //}
        }

        async void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (scrollViewer.ZoomFactor != currentPage.ZoomFactor)
            {
                if (!e.IsIntermediate)
                {
                    currentPage.ZoomFactor = scrollViewer.ZoomFactor;
                    await currentPage.RefreshImageAsync();
                }
            }
        }


        #region Inking Code

        InkManager m_InkManager = new Windows.UI.Input.Inking.InkManager();
        InkManager m_HighLightManager = new Windows.UI.Input.Inking.InkManager();

        private uint m_PenId;
        private uint _touchID;
        private Windows.Foundation.Point _previousContactPt;
        private Windows.Foundation.Point currentContactPt;
        private double x1;
        private double y1;
        private double x2;
        private double y2;

        private Color m_CurrentDrawingColor = Colors.Black;

        private double m_CurrentDrawingSize = 4;

        private Color m_CurrentHighlightColor = Colors.Yellow;

        private double m_CurrentHighlightSize = 8;

        private string m_CurrentMode = "Ink";

        private bool m_IsRecognizing = false;

        public InkManager CurrentManager
        {
            get
            {
                if (m_CurrentMode == "Ink") return m_InkManager;

                return m_HighLightManager;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            InkMode();

            InkCanvas.PointerPressed += new PointerEventHandler(OnCanvasPointerPressed);
            InkCanvas.PointerMoved += new PointerEventHandler(OnCanvasPointerMoved);
            InkCanvas.PointerReleased += new PointerEventHandler(OnCanvasPointerReleased);
            InkCanvas.PointerExited += new PointerEventHandler(OnCanvasPointerReleased);

            flipView.PointerReleased += new PointerEventHandler(OnCanvasPointerReleased);
            flipView.PointerExited += new PointerEventHandler(OnCanvasPointerReleased);
        }

        #region Pointer Event Handlers


        public void OnCanvasPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId == m_PenId)
            {
                //if (!App.isReadMode)
                //{
                    Windows.UI.Input.PointerPoint pt = e.GetCurrentPoint(InkCanvas);

                    if (m_CurrentMode == "Erase")
                    {
                        System.Diagnostics.Debug.WriteLine("Erasing : Pointer Released");

                        m_InkManager.ProcessPointerUp(pt);
                        m_HighLightManager.ProcessPointerUp(pt);
                    }
                    else
                    {
                        // Pass the pointer information to the InkManager. 
                        CurrentManager.ProcessPointerUp(pt);
                    }
                //}
            }
            else if (e.Pointer.PointerId == _touchID)
            {
                //if (!App.isReadMode)
                //{
                    // Process touch input 
                    Windows.UI.Input.PointerPoint pt = e.GetCurrentPoint(InkCanvas);

                    if (m_CurrentMode == "Erase")
                    {
                        System.Diagnostics.Debug.WriteLine("Erasing : Pointer Released");

                        m_InkManager.ProcessPointerUp(pt);
                        m_HighLightManager.ProcessPointerUp(pt);
                    }
                    else
                    {
                        // Pass the pointer information to the InkManager. 
                        CurrentManager.ProcessPointerUp(pt);
                    }
                //}
            }

            _touchID = 0;
            m_PenId = 0;

            // Call an application-defined function to render the ink strokes. 

            RefreshCanvas();

            e.Handled = true;
        }

        private void OnCanvasPointerMoved(object sender, PointerRoutedEventArgs e)
        {

            if (e.Pointer.PointerId == m_PenId)
            {
                PointerPoint pt = e.GetCurrentPoint(InkCanvas);

                // Render a red line on the canvas as the pointer moves.  
                // Distance() is an application-defined function that tests 
                // whether the pointer has moved far enough to justify  
                // drawing a new line. 
                currentContactPt = pt.Position;
                x1 = _previousContactPt.X;
                y1 = _previousContactPt.Y;
                x2 = currentContactPt.X;
                y2 = currentContactPt.Y;

                var color = m_CurrentMode == "Ink" ? m_CurrentDrawingColor : m_CurrentHighlightColor;
                var size = m_CurrentMode == "Ink" ? m_CurrentDrawingSize : m_CurrentHighlightSize;

                if (Distance(x1, y1, x2, y2) > 2.0 && m_CurrentMode != "Erase")
                {
                    Line line = new Line()
                    {
                        X1 = x1,
                        Y1 = y1,
                        X2 = x2,
                        Y2 = y2,
                        StrokeThickness = size,
                        Stroke = new SolidColorBrush(color)
                    };


                    if (m_CurrentMode == "Highlight") line.Opacity = 0.4;
                    _previousContactPt = currentContactPt;

                    // Draw the line on the canvas by adding the Line object as 
                    // a child of the Canvas object. 
                    InkCanvas.Children.Add(line);
                }

                if (m_CurrentMode == "Erase")
                {
                    System.Diagnostics.Debug.WriteLine("Erasing : Pointer Update");

                    m_InkManager.ProcessPointerUpdate(pt);
                    m_HighLightManager.ProcessPointerUpdate(pt);
                }
                else
                {
                    // Pass the pointer information to the InkManager. 
                    CurrentManager.ProcessPointerUpdate(pt);
                }
            }

            else if (e.Pointer.PointerId == _touchID)
            {
                //if (!App.isReadMode)
                //{
                    // Process touch input
                    PointerPoint pt = e.GetCurrentPoint(InkCanvas);

                    // Render a red line on the canvas as the pointer moves.  
                    // Distance() is an application-defined function that tests 
                    // whether the pointer has moved far enough to justify  
                    // drawing a new line. 
                    currentContactPt = pt.Position;
                    x1 = _previousContactPt.X;
                    y1 = _previousContactPt.Y;
                    x2 = currentContactPt.X;
                    y2 = currentContactPt.Y;

                    var color = m_CurrentMode == "Ink" ? m_CurrentDrawingColor : m_CurrentHighlightColor;
                    var size = m_CurrentMode == "Ink" ? m_CurrentDrawingSize : m_CurrentHighlightSize;

                    if (Distance(x1, y1, x2, y2) > 2.0 && m_CurrentMode != "Erase")
                    {
                        Line line = new Line()
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2,
                            StrokeThickness = size,
                            Stroke = new SolidColorBrush(color)
                        };


                        if (m_CurrentMode == "Highlight") line.Opacity = 0.4;
                        _previousContactPt = currentContactPt;

                        // Draw the line on the canvas by adding the Line object as 
                        // a child of the Canvas object. 
                        InkCanvas.Children.Add(line);
                    }

                    if (m_CurrentMode == "Erase")
                    {
                        System.Diagnostics.Debug.WriteLine("Erasing : Pointer Update");

                        m_InkManager.ProcessPointerUpdate(pt);
                        m_HighLightManager.ProcessPointerUpdate(pt);
                    }
                    else
                    {
                        // Pass the pointer information to the InkManager. 
                        CurrentManager.ProcessPointerUpdate(pt);
                    }
                //}
            }


        }

        private double Distance(double x1, double y1, double x2, double y2)
        {
            double d = 0;
            d = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            return d;
        }

        public void OnCanvasPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                // Get information about the pointer location. 
                PointerPoint pt = e.GetCurrentPoint(InkCanvas);
                _previousContactPt = pt.Position;

                // Accept input only from a pen or mouse with the left button pressed.  
                PointerDeviceType pointerDevType = e.Pointer.PointerDeviceType;
                if (pointerDevType == PointerDeviceType.Pen ||
                        pointerDevType == PointerDeviceType.Mouse &&
                        pt.Properties.IsLeftButtonPressed)
                {
                    if (m_CurrentMode == "Erase")
                    {
                        System.Diagnostics.Debug.WriteLine("Erasing : Pointer Pressed");

                        m_InkManager.ProcessPointerDown(pt);
                        m_HighLightManager.ProcessPointerDown(pt);
                    }
                    else
                    {
                        // Pass the pointer information to the InkManager. 
                        CurrentManager.ProcessPointerDown(pt);
                    }

                    m_PenId = pt.PointerId;

                    e.Handled = true;
                }

                else if (pointerDevType == PointerDeviceType.Touch)
                {
                    // Process touch input
                    //if (!App.isReadMode)
                    //{
                        if (m_CurrentMode == "Erase")
                        {
                            System.Diagnostics.Debug.WriteLine("Erasing : Pointer Pressed");

                            m_InkManager.ProcessPointerDown(pt);
                            m_HighLightManager.ProcessPointerDown(pt);
                        }
                        else
                        {
                            // Pass the pointer information to the InkManager. 
                            CurrentManager.ProcessPointerDown(pt);
                        }

                        m_PenId = pt.PointerId;

                        e.Handled = true;
                    //}
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Mode Functions

        // Change the color and width in the default (used for new strokes) to the values
        // currently set in the current context.
        private void SetDefaults(double strokeSize, Color color)
        {
            var newDrawingAttributes = new InkDrawingAttributes();
            newDrawingAttributes.Size = new Size(strokeSize, strokeSize);
            newDrawingAttributes.Color = color;
            newDrawingAttributes.FitToCurve = true;
            CurrentManager.SetDefaultDrawingAttributes(newDrawingAttributes);
        }

        private void HighlightMode()
        {
            m_CurrentMode = "Highlight";
            m_HighLightManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Inking;
            SetDefaults(m_CurrentHighlightSize, m_CurrentHighlightColor);
        }

        private void InkMode()
        {
            m_CurrentMode = "Ink";
            m_InkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Inking;
            SetDefaults(m_CurrentDrawingSize, m_CurrentDrawingColor);
        }

        private void SelectMode()
        {
            m_InkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Selecting;
        }

        private void EraseMode()
        {
            m_InkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Erasing;
            m_HighLightManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Erasing;
            m_CurrentMode = "Erase";
            //selCanvas.style.cursor = "url(images/erase.cur), auto";
        }

        //function tempEraseMode()
        //{
        //    saveMode();
        //    selContext.strokeStyle = "rgba(255,255,255,0.0)";
        //    context = selContext;
        //    inkManager.mode = inkManager.mode = Windows.UI.Input.Inking.InkManipulationMode.erasing;
        //    selCanvas.style.cursor = "url(images/erase.cur), auto";
        //}

        #endregion


        #region Rendering Functions

        private async void ReadInk(StorageFile storageFile)
        {
            if (storageFile != null)
            {
                using (var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    await m_InkManager.LoadAsync(stream);

                    if (m_InkManager.GetStrokes().Count > 0)
                    {
                        RenderStrokes();
                    }
                }
            }
        }

        private async void ReadInk_forHighLight(StorageFile storageFile)
        {
            if (storageFile != null)
            {
                using (var stream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    await m_HighLightManager.LoadAsync(stream);

                    if (m_HighLightManager.GetStrokes().Count > 0)
                    {
                        RenderStrokes_forHighLight();
                    }
                }
            }
        }


        private void RenderStroke(InkStroke stroke, Color color, double width, double opacity = 1)
        {
            var renderingStrokes = stroke.GetRenderingSegments();
            var path = new Windows.UI.Xaml.Shapes.Path();
            path.Data = new PathGeometry();
            ((PathGeometry)path.Data).Figures = new PathFigureCollection();
            var pathFigure = new PathFigure();
            pathFigure.StartPoint = renderingStrokes.First().Position;
            ((PathGeometry)path.Data).Figures.Add(pathFigure);
            foreach (var renderStroke in renderingStrokes)
            {
                pathFigure.Segments.Add(new BezierSegment()
                {
                    Point1 = renderStroke.BezierControlPoint1,
                    Point2 = renderStroke.BezierControlPoint2,
                    Point3 = renderStroke.Position
                });
            }

            path.StrokeThickness = width;
            path.Stroke = new SolidColorBrush(color);

            path.Opacity = opacity;

            InkCanvas.Children.Add(path);
        }

        private void RenderStroke_forHighLight(InkStroke stroke, Color color, double width, double opacity = 1)
        {
            var renderingStrokes = stroke.GetRenderingSegments();
            var path = new Windows.UI.Xaml.Shapes.Path();
            path.Data = new PathGeometry();
            ((PathGeometry)path.Data).Figures = new PathFigureCollection();
            var pathFigure = new PathFigure();
            pathFigure.StartPoint = renderingStrokes.First().Position;
            ((PathGeometry)path.Data).Figures.Add(pathFigure);
            foreach (var renderStroke in renderingStrokes)
            {
                pathFigure.Segments.Add(new BezierSegment()
                {
                    Point1 = renderStroke.BezierControlPoint1,
                    Point2 = renderStroke.BezierControlPoint2,
                    Point3 = renderStroke.Position
                });
            }

            path.StrokeThickness = width;
            path.Stroke = new SolidColorBrush(color);

            path.Opacity = opacity;

            HighLightCanvas.Children.Add(path);

        }


        private void RenderStrokes()
        {
            var strokes = m_InkManager.GetStrokes();

            var highlightStrokes = m_HighLightManager.GetStrokes();

            foreach (var stroke in strokes)
            {
                if (stroke.Selected)
                {
                    RenderStroke(stroke, stroke.DrawingAttributes.Color, stroke.DrawingAttributes.Size.Width * 2);
                }
                else
                {
                    RenderStroke(stroke, stroke.DrawingAttributes.Color, stroke.DrawingAttributes.Size.Width);
                }
            }

            foreach (var stroke in highlightStrokes)
            {
                if (stroke.Selected)
                {
                    RenderStroke(stroke, stroke.DrawingAttributes.Color, stroke.DrawingAttributes.Size.Width * 2, 0.4);
                }
                else
                {
                    RenderStroke(stroke, stroke.DrawingAttributes.Color, stroke.DrawingAttributes.Size.Width, 0.4);
                }
            }
        }

        private void RenderStrokes_forHighLight()
        {
            var highlightStrokes = m_HighLightManager.GetStrokes();

            foreach (var stroke in highlightStrokes)
            {
                if (stroke.Selected)
                {
                    RenderStroke_forHighLight(stroke, stroke.DrawingAttributes.Color, stroke.DrawingAttributes.Size.Width * 2, 0.4);
                }
                else
                {
                    RenderStroke_forHighLight(stroke, stroke.DrawingAttributes.Color, stroke.DrawingAttributes.Size.Width, 0.4);
                }
            }
        }

        private void RefreshCanvas()
        {
            InkCanvas.Children.Clear();
            //InkCanvas.Children.Add(flipView);

            HighLightCanvas.Children.Clear();


            RenderStrokes();
            RenderStrokes_forHighLight();

            if (m_IsRecognizing && m_InkManager.GetStrokes().Count > 0)
            {
                //Display.Text = string.Empty;

                var recognizers = m_InkManager.GetRecognizers();

                m_InkManager.SetDefaultRecognizer(recognizers.First());

                var task = m_InkManager.RecognizeAsync(InkRecognitionTarget.All);

                task.Completed = new AsyncOperationCompletedHandler<IReadOnlyList<InkRecognitionResult>>((operation, status) =>
                {
                    double previousX = 0;
                    double previousY = 0;

                    var firstWord = true;

                    foreach (var result in operation.GetResults())
                    {
                        var isNewLine = false;
                        var isNewWord = false;

                        if (Math.Abs(result.BoundingRect.Left - previousX) > 10) isNewWord = true;

                        if (Math.Abs(result.BoundingRect.Bottom - previousY) > 20) isNewLine = true;

                        previousX = result.BoundingRect.Right;
                        previousY = result.BoundingRect.Bottom;

                        //Display.Text += isNewLine && !firstWord ? "\r\n" : string.Empty;
                        //Display.Text += isNewWord && !firstWord ? " " : string.Empty;

                        //Display.Text += result.GetTextCandidates().First();

                        firstWord = false;
                    }
                });
            }
        }

        #endregion

        #region Additional Commands

        private async void Load(object sender, RoutedEventArgs e)
        {
            try
            {
                Windows.Storage.Pickers.FileOpenPicker open = new Windows.Storage.Pickers.FileOpenPicker();
                open.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                open.FileTypeFilter.Add(".png");
                StorageFile filesave = await open.PickSingleFileAsync();

                ReadInk(filesave);
            }
            catch (Exception ex)
            {

                var dlge = new MessageDialog(ex.Message);
                dlge.ShowAsync();
            }


        }

        private async void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                Windows.Storage.Pickers.FileSavePicker save = new Windows.Storage.Pickers.FileSavePicker();
                save.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
                save.DefaultFileExtension = ".png";
                save.FileTypeChoices.Add("PNG", new string[] { ".png" });
                StorageFile filesave = await save.PickSaveFileAsync();
                using (IOutputStream ab = await filesave.OpenAsync(FileAccessMode.ReadWrite))
                {
                    if (ab != null)
                        await m_InkManager.SaveAsync(ab);
                }
            }
            catch (Exception ex)
            {
                var dlge = new MessageDialog(ex.Message);
                dlge.ShowAsync();
            }


        }

        private async void Copy(object sender, RoutedEventArgs e)
        {
            var strokes = m_InkManager.GetStrokes();

            for (int i = 0; i < strokes.Count; i++)
            {
                strokes[i].Selected = true;
            }

            m_InkManager.CopySelectedToClipboard();

            var msgdlg = new MessageDialog("Copied to clipboard successfully");
            await msgdlg.ShowAsync();

        }

        private async void Paste(object sender, RoutedEventArgs e)
        {

            var canpaste = m_InkManager.CanPasteFromClipboard();
            if (canpaste)
            {
                // panelcanvas.Children.Clear(); 
                m_InkManager.PasteFromClipboard(_previousContactPt);
                var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                if (dataPackageView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Bitmap))
                {
                    await dataPackageView.GetBitmapAsync();
                    RandomAccessStreamReference rv = await dataPackageView.GetBitmapAsync();
                    IRandomAccessStream irac = await rv.OpenReadAsync();
                    BitmapImage img = new BitmapImage();
                    img.SetSource(irac);
                    //image.Source = img;

                }
            }
            else
            {
                var msgdlg = new MessageDialog("Clipboard is empty or unable to paste from clipboard");
                msgdlg.ShowAsync();
            }
        }

        private void ClearAll_onClick(object sender, RoutedEventArgs e)
        {
            m_InkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Erasing;

            var strokes = m_InkManager.GetStrokes();
            var HighLightStrokes = m_HighLightManager.GetStrokes();

            for (int i = 0; i < strokes.Count; i++)
            {
                strokes[i].Selected = true;
            }

            for (int i = 0; i < HighLightStrokes.Count; i++)
            {
                HighLightStrokes[i].Selected = true;
            }

            m_InkManager.DeleteSelected();
            m_HighLightManager.DeleteSelected();

            InkCanvas.Children.Clear();
            HighLightCanvas.Children.Clear();
            //InkCanvas.Children.Add(flipView);

            m_InkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Inking;

            //InkCanvas.Background = new SolidColorBrush(Colors.White);
            //InkCanvas.Children.Clear();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            RefreshCanvas();
        }

        #endregion

        #region Inking Functions

        //private void Recognize(object sender, RoutedEventArgs e)
        //{
        //    m_IsRecognizing = !m_IsRecognizing;
        //    RefreshCanvas();
        //}

        private void Select(object sender, RoutedEvent e)
        {
            SelectMode();
        }

        private void Erase(object sender, RoutedEventArgs e)
        {
            App.isReadMode = false;

            m_InkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Erasing;

            var strokes = m_InkManager.GetStrokes();

            m_InkManager.DeleteSelected();

            EraseMode();
        }

        #endregion

        #region Flyout Context Menus

        Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Windows.Foundation.Point point = buttonTransform.TransformPoint(new Windows.Foundation.Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        private async void SelectColor(object sender, RoutedEventArgs e)
        {
            App.isReadMode = false;
            try
            {
                var menu = new PopupMenu();
                menu.Commands.Add(new UICommand("Black", null, 1));
                //menu.Commands.Add(new UICommandSeparator());
                menu.Commands.Add(new UICommand("Blue", null, 2));
                //menu.Commands.Add(new UICommandSeparator());
                menu.Commands.Add(new UICommand("Red", null, 3));
                menu.Commands.Add(new UICommand("Green", null, 4));

                System.Diagnostics.Debug.WriteLine("Context Menu is opening");

                var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender));

                if (chosenCommand != null)
                {
                    switch ((int)chosenCommand.Id)
                    {
                        case 1:
                            m_CurrentDrawingColor = Colors.Black;
                            break;
                        case 2:
                            m_CurrentDrawingColor = Colors.Blue;
                            break;
                        case 3:
                            m_CurrentDrawingColor = Colors.Red;
                            break;
                        case 4:
                            m_CurrentDrawingColor = Colors.Green;
                            break;
                    }

                    InkMode();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void SelectHighlightColor(object sender, RoutedEventArgs e)
        {
            App.isReadMode = false;
            try
            {
                var menu = new PopupMenu();
                menu.Commands.Add(new UICommand("Yellow", null, 1));
                //menu.Commands.Add(new UICommandSeparator());
                menu.Commands.Add(new UICommand("Aqua", null, 2));
                //menu.Commands.Add(new UICommandSeparator());
                menu.Commands.Add(new UICommand("Line", null, 3));

                System.Diagnostics.Debug.WriteLine("Context Menu is opening");

                var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender));

                if (chosenCommand != null)
                {
                    switch ((int)chosenCommand.Id)
                    {
                        case 1:
                            m_CurrentHighlightColor = Colors.Yellow;
                            break;
                        case 2:
                            m_CurrentHighlightColor = Colors.Aqua;
                            break;
                        case 3:
                            m_CurrentHighlightColor = Colors.Lime;
                            break;
                    }

                    HighlightMode();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void SelectSize(object sender, RoutedEventArgs e)
        {
            App.isReadMode = false;
            try
            {
                var menu = new PopupMenu();
                menu.Commands.Add(new UICommand("Smallest", null, 0));
                menu.Commands.Add(new UICommand("Small", null, 1));
                //menu.Commands.Add(new UICommandSeparator());
                menu.Commands.Add(new UICommand("Medium", null, 2));
                //menu.Commands.Add(new UICommandSeparator());
                menu.Commands.Add(new UICommand("Large", null, 3));
                menu.Commands.Add(new UICommand("Largest", null, 4));

                System.Diagnostics.Debug.WriteLine("Context Menu is opening");

                var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender));

                if (chosenCommand != null)
                {
                    switch ((int)chosenCommand.Id)
                    {
                        case 0:
                            m_CurrentDrawingSize = 2;
                            break;
                        case 1:
                            m_CurrentDrawingSize = 4;
                            break;
                        case 2:
                            m_CurrentDrawingSize = 6;
                            break;
                        case 3:
                            m_CurrentDrawingSize = 8;
                            break;
                        case 4:
                            m_CurrentDrawingSize = 10;
                            break;
                    }

                    InkMode();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void SelectHighlightSize(object sender, RoutedEventArgs e)
        {
            App.isReadMode = false;
            try
            {
                var menu = new PopupMenu();
                menu.Commands.Add(new UICommand("Small", null, 0));
                menu.Commands.Add(new UICommand("Medium", null, 1));
                menu.Commands.Add(new UICommand("Large", null, 2));

                System.Diagnostics.Debug.WriteLine("Context Menu is opening");

                var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender));

                if (chosenCommand != null)
                {
                    switch ((int)chosenCommand.Id)
                    {
                        case 0:
                            m_CurrentHighlightSize = 8;
                            break;
                        case 1:
                            m_CurrentHighlightSize = 12;
                            break;
                        case 2:
                            m_CurrentHighlightSize = 16;
                            break;
                    }

                    HighlightMode();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void SelectAction(object sender, RoutedEventArgs e)   // This Method is for save button in TopAppBar
        {
            string currentPageFileName = flipView.SelectedIndex.ToString() + ".png";
            string currentPageFileName_HighLight = flipView.SelectedIndex.ToString() + "_HighLight.png";

            if (!await DoesFileExist(currentPageFileName) || !await DoesFileExist(currentPageFileName_HighLight))
            {
                try
                {
                    var abc = m_InkManager.GetStrokes();
                    var abc_HighLight = m_HighLightManager.GetStrokes();

                    StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(App.FileDisplayName, CreationCollisionOption.OpenIfExists);

                    #region save strokes -> Pen Ink

                    if (abc.Count == 0)
                    {
                        //return;
                    }
                    else
                    {
                        StorageFile fileToSave;

                        fileToSave = await storageFolder.CreateFileAsync(currentPageFileName, CreationCollisionOption.ReplaceExisting);

                        using (IOutputStream ab = await fileToSave.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            if (ab != null)
                                await m_InkManager.SaveAsync(ab);
                        }
                    }

                    #endregion

                    #region save strokes -> HighLight Ink

                    if (abc_HighLight.Count == 0)
                    {
                        //return;
                    }
                    else
                    {
                        StorageFile fileToSave_HighLight;
                        //StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(App.FileDisplayName, CreationCollisionOption.OpenIfExists);

                        fileToSave_HighLight = await storageFolder.CreateFileAsync(currentPageFileName_HighLight, CreationCollisionOption.ReplaceExisting);

                        using (IOutputStream ab = await fileToSave_HighLight.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            if (ab != null)
                                await m_HighLightManager.SaveAsync(ab);
                        }
                    }

                    #endregion

                    if (abc.Count == 0 && abc_HighLight.Count == 0)
                    {
                        if (await DoesFileExist(currentPageFileName))
                        {
                            var InkFile = await storageFolder.GetFileAsync(currentPageFileName);
                            await InkFile.DeleteAsync();
                        }
                        if (await DoesFileExist(currentPageFileName_HighLight))
                        {
                            var HighLightFile = await storageFolder.GetFileAsync(currentPageFileName_HighLight);
                            await HighLightFile.DeleteAsync();
                        }
                    }

                    MessageDialog ms = new MessageDialog("Annotations saved successfully..!!!", "PDF with Annotation");
                    await ms.ShowAsync();

                }
                catch (Exception ex)    // This Exception shows that the image with the same name already exist in localFolder
                {
                    var dlge = new MessageDialog(ex.Message);
                    dlge.ShowAsync();
                }
            }

            #region commented -> flyout menu method
            //try
            //{
            //    var menu = new PopupMenu();
            //    menu.Commands.Add(new UICommand("Copy", null, 0));
            //    menu.Commands.Add(new UICommand("Paste", null, 1));
            //    menu.Commands.Add(new UICommand("Save", null, 2));
            //    //menu.Commands.Add(new UICommand("Load", null, 3));
            //    menu.Commands.Add(new UICommand("Refresh", null, 4));
            //    menu.Commands.Add(new UICommand("Clear", null, 5));

            //    System.Diagnostics.Debug.WriteLine("Context Menu is opening");

            //    var chosenCommand = await menu.ShowForSelectionAsync(GetElementRect((FrameworkElement)sender));

            //    if (chosenCommand != null)
            //    {
            //        switch ((int)chosenCommand.Id)
            //        {
            //            case 0:
            //                Copy(sender, e);
            //                break;
            //            case 1:
            //                Paste(sender, e);
            //                break;
            //            case 2:
            //                Save(sender, e);
            //                break;
            //            case 3:
            //                Load(sender, e);
            //                break;
            //            case 4:
            //                Refresh(sender, e);
            //                break;
            //            case 5:
            //                Clear(sender, e);
            //                break;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            #endregion
        }

        #endregion

        private void ReadMode_OnClick(object sender, RoutedEventArgs e)
        {
            App.isReadMode = true;
        }

        #endregion

    }
}
