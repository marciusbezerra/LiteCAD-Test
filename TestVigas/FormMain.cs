using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestVigas
{
    public partial class FormMain : Form
    {
        // tem arquivos em C:\Users\marci\source\repos\TestVigas\TestVigas\files para guardar em alguma parte
        public FormMain()
        {
            InitializeComponent();
            Lcad.Initialize();
            Lcad.PropPutStr(0, 1, "12345");
        }

        int hWnd, hCmd, hProp, hDrw;
        int hModelSpace;

        List<IMyClassBase> ObjList = new List<IMyClassBase>();

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ResizeScreen();
        }

        private void splitContainer2_SplitterMoved(object sender, SplitterEventArgs e)
        {
            ResizeScreen();
        }

        private void toolStripButtonLine_Click(object sender, EventArgs e)
        {
            Lcad.WndExeCommand(hWnd, Lcad.LC_CMD_LINE, 0);
        }

        private void toolStripButtonRetangle_Click(object sender, EventArgs e)
        {
            Lcad.WndExeCommand(hWnd, Lcad.LC_CMD_RECT, 0);
        }

        private void toolStripButtonCircle_Click(object sender, EventArgs e)
        {
            Lcad.WndExeCommand(hWnd, Lcad.LC_CMD_CIRCLE, 0);
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            Layares();
        }

        F_PROPCHANGED pFunc;
        F_SELECTION pFuncSelection;

        private void PropChangedProc(int hDrw, int idProp, int hObj)
        {
            Debug.WriteLine($"hDrw: {hDrw} idProp: {idProp} hObj: {hObj}");
        }

        private void SelectionProc(int hDrw, int hBlock, bool bSelect, int nEnts, int hEnt, int nTotal)
        {
            if (hEnt > 0)
            {
                var entId = Lcad.PropGetInt(hEnt, Lcad.LC_PROP_ENT_ID);
                SelectObj(entId);
            }
            else
                propertyGrid1.SelectedObject = null;
        }

        private void SelectObj(int entId)
        {
            var obj = ObjList.FirstOrDefault(o => o.Id == entId);
            if (obj == null)
            {
                obj = new MyClassDesconhecido(entId) { Tipo = ObjType.Desconhecido };
                ObjList.Add(obj);
            }

            var index = ObjList.IndexOf(obj);

            switch (obj.Tipo)
            {
                case ObjType.Pilar:
                    if (!(ObjList[index] is MyClassPilar)) ObjList[index] = new MyClassPilar(entId) { Tipo = ObjType.Pilar };
                    break;
                case ObjType.Piso:
                    if (!(ObjList[index] is MyClassPiso)) ObjList[index] = new MyClassPiso(entId) { Tipo = ObjType.Piso };
                    break;
                case ObjType.Large:
                    if (!(ObjList[index] is MyClassLarge)) ObjList[index] = new MyClassLarge(entId) { Tipo = ObjType.Large };
                    break;
                default:
                    if (!(ObjList[index] is MyClassDesconhecido)) ObjList[index] = new MyClassDesconhecido(entId) { Tipo = ObjType.Desconhecido };
                    break;
            }
            propertyGrid1.SelectedObject = ObjList[index];
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            hWnd = Lcad.CreateWindow(splitContainer2.Panel1.Handle, Lcad.LC_WS_DEFAULT, 0, 0, splitContainer2.Panel1.Width, splitContainer2.Panel1.Height);
            hCmd = Lcad.CreateCmdwin(splitContainer2.Panel2.Handle, 0, 0, splitContainer2.Panel2.Width, splitContainer2.Panel2.Height);
            hProp = Lcad.CreatePropwin(splitContainerProperties.Panel1.Handle, 0, 0, splitContainerProperties.Panel1.Width, splitContainerProperties.Panel1.Height);

            Lcad.WndSetCmdwin(hWnd, hCmd);
            Lcad.WndSetPropwin(hWnd, hProp);

            hDrw = Lcad.CreateDrawing();

            Lcad.DrwNew(hDrw, "Calculo Colunas", hWnd);

            pFunc = new F_PROPCHANGED(PropChangedProc);
            Lcad.OnEventPropChanged(pFunc);

            pFuncSelection = new F_SELECTION(SelectionProc);
            Lcad.OnEventSelection(pFuncSelection);

            ResizeScreen();


            hModelSpace = Lcad.DrwGetFirstObject(hDrw, Lcad.LC_OBJ_BLOCK);

            var hView = Lcad.DrwGetFirstObject(hDrw, Lcad.LC_OBJ_VIEW);

            //LC_PROP_VIEW_GRID = 1381;
            //LC_PROP_VIEW_GRIDSNAP = 1382;
            //LC_PROP_VIEW_OSNAP = 1383;
            //LC_PROP_VIEW_OSNAPMODE = 1384;

            Lcad.PropPutBool(hView, Lcad.LC_PROP_VIEW_GRID, true);
            Lcad.PropPutBool(hView, Lcad.LC_PROP_VIEW_GRIDSNAP, true);
            Lcad.PropPutBool(hView, Lcad.LC_PROP_VIEW_OSNAP, true);
            Lcad.PropPutInt(hView, Lcad.LC_PROP_VIEW_OSNAPMODE, Lcad.LC_SNAP_ALL);
            //Layares();

        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.PropertyDescriptor.PropertyType == typeof(ObjType))
            {
                var obj = propertyGrid1.SelectedObject as IMyClassBase;
                SelectObj(obj.Id);
            }
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            ResizeScreen();
        }

        private void Layares()
        {
            Lcad.DrwAddLayer(hDrw, "LayerText", "1", 0, Lcad.LC_LWEIGHT_000);
            Lcad.DrwAddLayer(hDrw, "LayerLine", "2", 0, Lcad.LC_LWEIGHT_000);
            Lcad.DrwAddLayer(hDrw, "LayerPLine", "3", 0, Lcad.LC_LWEIGHT_000);
            Lcad.DrwAddLayer(hDrw, "LayerPolygon", "4", 0, Lcad.LC_LWEIGHT_000);
            Lcad.DrwAddLayer(hDrw, "LayerCircle", "5", 0, Lcad.LC_LWEIGHT_000);
            Lcad.DrwAddLayer(hDrw, "LayerArc", "6", 0, Lcad.LC_LWEIGHT_000);
            Lcad.DrwAddLayer(hDrw, "LayerRect", "7", 0, Lcad.LC_LWEIGHT_000);

            var hEnt = Lcad.BlockGetFirstEnt(hModelSpace);

            while (hEnt != 0)
            {
                switch (Lcad.PropGetInt(hEnt, Lcad.LC_PROP_ENT_TYPE))
                {
                    case Lcad.LC_ENT_TEXT:
                        Lcad.PropPutStr(hEnt, Lcad.LC_PROP_ENT_LAYER, "LayerText");
                        break;
                    case Lcad.LC_ENT_LINE:
                        Lcad.PropPutStr(hEnt, Lcad.LC_PROP_ENT_LAYER, "LayerLine");
                        break;
                    case Lcad.LC_ENT_POLYLINE:
                        Lcad.PropPutStr(hEnt, Lcad.LC_PROP_ENT_LAYER, "LayerPLine");
                        break;
                    case Lcad.LC_ENT_CIRCLE:
                        Lcad.PropPutStr(hEnt, Lcad.LC_PROP_ENT_LAYER, "LayerCircle");
                        break;
                    case Lcad.LC_ENT_ARC:
                        Lcad.PropPutStr(hEnt, Lcad.LC_PROP_ENT_LAYER, "LayerArc");
                        break;
                    case Lcad.LC_ENT_RECT:
                        Lcad.PropPutStr(hEnt, Lcad.LC_PROP_ENT_LAYER, "LayerRect");
                        break;
                }
                hEnt = Lcad.BlockGetNextEnt(hModelSpace, hEnt);
            }

            Lcad.DrwRegenViews(hDrw, 0);
        }

        private void ResizeScreen()
        {
            Lcad.WndResize(hWnd, 0, 0, splitContainer2.Panel1.Width, splitContainer2.Panel1.Height);
            Lcad.CmdwinResize(hCmd, 0, 0, splitContainer2.Panel2.Width, splitContainer2.Panel2.Height);
            Lcad.PropwinResize(hProp, 0, 0, splitContainerProperties.Panel1.Width, splitContainerProperties.Panel1.Height);
        }
    }

    public enum ObjType
    {
        Desconhecido,
        Pilar,
        Piso,
        Large
    }

    public interface IMyClassBase
    {
        ObjType Tipo { get; set; }
        int Id { get; }
        //public MyClassDesconhecido Desconhecido { get; set; }
        //public MyClassPilar Pilar { get; set; }
        //public MyClassPiso Piso { get; set; }
        //public MyClassLarge Large { get; set; }
    }

    public class MyClassDesconhecido : IMyClassBase
    {
        public ObjType Tipo { get; set; }
        public int Id { get; }
        public MyClassDesconhecido(int id)
        {
            Id = id;
        }
    }

    public class MyClassPilar : IMyClassBase
    {
        public ObjType Tipo { get; set; }
        public int Id { get; }
        public float Forca { get; set; }
        public float Resitencia { get; set; }
        public MyClassPilar(int id)
        {
            Id = id;
        }
    }

    public class MyClassPiso : IMyClassBase
    {
        public ObjType Tipo { get; set; }
        public int Id { get; }
        public decimal Peso { get; set; }
        public string Material { get; set; }
        public MyClassPiso(int id)
        {
            Id = id;
        }
    }

    public class MyClassLarge : IMyClassBase
    {
        [Description("Tipo do Estrutura")]
        public ObjType Tipo { get; set; }
        public int Id { get; }
        public decimal Tamanho { get; set; }
        public decimal Profundidade { get; set; }
        public MyClassLarge(int id)
        {
            Id = id;
        }
    }
}
