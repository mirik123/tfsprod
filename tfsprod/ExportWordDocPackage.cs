using Microsoft.TeamFoundation.Controls;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking.Extensibility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using tfsprod;
using MSWord = Microsoft.Office.Interop.Word;

namespace TFSExt.ExportWordDoc
{
    public static class ExportWordDocPackage
    {
        private class Tag: IEquatable<Tag>
        {
            public int tWorkItemID, tlevel, tParentID;
            public string tLinkTypeID;
            public string tPrefix;
        
            public bool  Equals(Tag other)
            {
                return tWorkItemID == other.tWorkItemID && (other.tlevel < 0 ? true : tlevel == other.tlevel);
            }
        }

        /// <summary>
        /// Menus the item callback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public static void TeamExpQueryCallback(object sender, EventArgs e)
        {
            var origCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            
            //IntPtr hier;
            //uint itemid;
            //IVsMultiItemSelect dummy;
            //string canonicalName;
            bool bcanceled;
            int icanceled;
            string OperationCaption = "Exporting Work Item query to Microsoft Word document";
            IVsThreadedWaitDialog2 dlg = null;
            
            try
            {
                dlg = Utilities.CreateThreadedWaitDialog(OperationCaption, "Parsing query...", "status", 100);
                dlg.UpdateProgress(OperationCaption, "Parsing query...", "status", 1, 100, false, out bcanceled);

                //Utilities.vsTeamExp.TeamExplorerWindow.GetCurrentSelection(out hier, out itemid, out dummy);
                //IVsHierarchy hierarchy = (IVsHierarchy)Marshal.GetObjectForIUnknown(hier);
                //Marshal.Release(hier);

                //hierarchy.GetProperty(itemid, (int)__VSHPROPID.VSHPROPID_ExtSelectedItem, out res);
                //MessageBox.Show(res.GetType().ToString());

                //hierarchy.GetCanonicalName(itemid, out canonicalName);

                /*
                string[] tokens = canonicalName.Split('/');
                string ProjectName = tokens[1];
                var proj = Utilities.wistore.Projects[ProjectName];

                QueryItem qItem = proj.QueryHierarchy;

                int currentTokenIndex = 2;
                while (currentTokenIndex < tokens.Length)
                {
                    qItem = (qItem as QueryFolder)[tokens[currentTokenIndex]];
                    currentTokenIndex++;
                }*/

                var WIQueriesPageExt = Utilities.teamExplorer.CurrentPage.GetService<IWorkItemQueriesExt>();
                var qItem = WIQueriesPageExt.SelectedQueryItems.First();

                //string[] qheader;
                string[][] qdata;
                bcanceled = ExecuteQueryLimitedFields(dlg, qItem as QueryDefinition, qItem.Project.Name, out qdata);
                dlg.UpdateProgress(OperationCaption, "Creating new Word document...", "status", 1, 100, false, out bcanceled);

                CreateWordDocParagraph((qItem as QueryDefinition).Name, qdata, dlg);

                Cursor.Current = origCursor;
                dlg.EndWaitDialog(out icanceled);
            }
            catch (Exception ex)
            {
                Cursor.Current = origCursor;
                if(dlg != null) dlg.EndWaitDialog(out icanceled);
                Utilities.OutputCommandString(ex.ToString());
                MessageBox.Show(ex.InnerException != null ? ex.InnerException.Message : ex.Message, Utilities.AppTitle, MessageBoxButtons.OK);
            }  
        }

        public static void CreateWordDocTable(string[] qheader, string[][] qdata)
        {
            var application = new MSWord.Application();
            var document = new MSWord.Document();
            //object missing = System.Reflection.Missing.Value;

            application.Visible = true;
            application.DisplayAlerts = MSWord.WdAlertLevel.wdAlertsNone;
            document = application.Documents.Add();
            document.PageSetup.Orientation = MSWord.WdOrientation.wdOrientLandscape;
            document.PageSetup.LeftMargin = 20;
            document.PageSetup.RightMargin = 20;
            document.PageSetup.TopMargin = 20;
            document.PageSetup.BottomMargin = 20;
            document.PageSetup.PageWidth = qheader.Length * 100 + 200 + 40;

            MSWord.Table qtable = document.Tables.Add(document.Range(), qdata.Length+1, qheader.Length);
            MSWord.Style styl = CreateTableStyle(ref document);
            qtable.Range.set_Style(styl);
            qtable.Borders[MSWord.WdBorderType.wdBorderBottom].LineStyle = MSWord.WdLineStyle.wdLineStyleDouble;
            qtable.Columns.Width = 100;
            qtable.Columns[1].Width = 300;
            
            for (int i = 0; i < qheader.Length; i++)
            {
                qtable.Cell(1, i+1).Range.Text = qheader[i];
            }

            for (int j = 0; j < qdata.Length; j++)
            {
                //int level = Int16.Parse(qdata[j][0]);
                //qtable.Cell(j + 2, 1).Range.

                for (int i = 0; i < qheader.Length; i++)
                {
                    qtable.Cell(j+2, i+1).Range.Text = qdata[j][i];
                }
            }

            Marshal.ReleaseComObject(document);
            Marshal.ReleaseComObject(application);
        }

        public static bool CreateWordDocParagraph(string qname, string[][] qdata, IVsThreadedWaitDialog2 dlg)
        {
            //InitializeVariables();
            System.Net.WebClient webclient = new System.Net.WebClient();
            webclient.Credentials = System.Net.CredentialCache.DefaultCredentials;
            
            var application = new MSWord.Application();
            var document = new MSWord.Document();
            object missing = System.Reflection.Missing.Value;
            int pagewidth = 800;
            bool bcanceled;
            int progress = 1;
            StringBuilder strbld = new StringBuilder();

            MSWord.WdBuiltinStyle[] hstyles = new MSWord.WdBuiltinStyle[8];
            hstyles[0] = MSWord.WdBuiltinStyle.wdStyleHeading1;
            hstyles[1] = MSWord.WdBuiltinStyle.wdStyleHeading2;
            hstyles[2] = MSWord.WdBuiltinStyle.wdStyleHeading3;
            hstyles[3] = MSWord.WdBuiltinStyle.wdStyleHeading4;
            hstyles[4] = MSWord.WdBuiltinStyle.wdStyleHeading5;
            hstyles[5] = MSWord.WdBuiltinStyle.wdStyleHeading6;
            hstyles[6] = MSWord.WdBuiltinStyle.wdStyleHeading7;
            hstyles[7] = MSWord.WdBuiltinStyle.wdStyleHeading8;

            application.Visible = true;
            application.WindowState = MSWord.WdWindowState.wdWindowStateMinimize;
            application.DisplayAlerts = MSWord.WdAlertLevel.wdAlertsNone;
            document = application.Documents.Add();
            //document.PageSetup.Orientation = MSWord.WdOrientation.wdOrientLandscape;
            document.PageSetup.LeftMargin = 20;
            document.PageSetup.RightMargin = 20;
            document.PageSetup.TopMargin = 20;
            document.PageSetup.BottomMargin = 20;
            document.PageSetup.PageWidth = pagewidth + 40;

            MSWord.Paragraph prg = document.Paragraphs.Add();
            prg.Range.Text = "Query results for " + qname + " [" + DateTime.Now + "]";
            prg.Range.set_Style(MSWord.WdBuiltinStyle.wdStyleTitle);
            //prg.Range.ParagraphFormat.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphCenter;
            prg.SpaceAfter = 100;
            prg.SpaceBefore = 100;
            prg.Range.InsertParagraphAfter();

            prg = document.Paragraphs.Add();
            prg.Range.Text = "Table of Contents";
            prg.Range.set_Style(MSWord.WdBuiltinStyle.wdStyleTocHeading);
            prg.Range.InsertParagraphAfter();

            prg = document.Paragraphs.Add();
            prg.Range.Text = "TOC";
            prg.Range.InsertParagraphAfter();
            prg.Range.InsertBreak(MSWord.WdBreakType.wdPageBreak);

            prg = document.Paragraphs.Add();
            MSWord.Table qtable = document.Tables.Add(prg.Range, qdata.Length, 1);
            prg.Range.InsertParagraphAfter();

            prg = document.Paragraphs.Add();
            prg.Range.Text = "Appendix";
            prg.Range.set_Style(MSWord.WdBuiltinStyle.wdStyleTitle);
            prg.Range.InsertParagraphAfter();

            object styleTypeTable = MSWord.WdStyleType.wdStyleTypeTable;
            MSWord.Style styl = document.Styles.Add("New Table Style", ref styleTypeTable);
            styl.ParagraphFormat.LineSpacingRule = MSWord.WdLineSpacing.wdLineSpaceSingle;
            styl.ParagraphFormat.SpaceAfter = 0;
            styl.ParagraphFormat.SpaceBefore = 0;
            styl.Table.TopPadding = 0;
            styl.Table.BottomPadding = 0;
            styl.Table.LeftPadding = 0;
            styl.Table.RightPadding = 0;
            //styl.Table.Borders.Enable = 1;
            qtable.Range.set_Style(styl);

            MSWord.Cell cell = qtable.Cell(1, 1);

            int headerwidth = 85;
            int levelwidth = 100;

            object rows, cols;           
            for (int i = 0; i < qdata.Length; i++)
            {   
                int level = int.Parse(qdata[i][1]);
                if(level > 0) 
                {
                    rows = 1;
                    cols = 2;
                    cell.Split(ref rows, ref cols);
                    cell.Range.Cells.SetWidth(level * levelwidth, MSWord.WdRulerStyle.wdAdjustSameWidth);
                    cell = cell.Next;
                }

                rows = 1 + (string.IsNullOrWhiteSpace(qdata[i][0]) ? 0 : 1) +(string.IsNullOrWhiteSpace(qdata[i][6]) ? 0 : 1);
                cols = 2;
                cell.Split(ref rows, ref cols);
                cell.Merge(cell.Next);

                string title = String.Format("{0} {1} ({2})",
                    qdata[i][2],
                    (qdata[i][5].Length > 128 ? qdata[i][5].Remove(128) : qdata[i][5]).Replace("\n", "").Replace("\r", "").Replace("\t", ""),
                    qdata[i][4]);
               
                cell.Range.Text = title;
                cell.Range.Font.Bold = 1;
                cell.Range.set_Style(hstyles[level<8 ? level:7]); 
                cell = cell.Next;

                dlg.UpdateProgress("Exporting Work Item query to Microsoft Word document", "Adding to Word document " + qdata[i][3] + " #" + qdata[i][4], "status", progress++, 100, false, out bcanceled);
                if (progress == 100) progress = 0;
                if (bcanceled)
                {
                    application.Visible = true;
                    Marshal.ReleaseComObject(document);
                    Marshal.ReleaseComObject(application);

                    return true;
                }

                /*cell.Range.Text = "Title";
                cell.Range.Cells.SetWidth(headerwidth, MSWord.WdRulerStyle.wdAdjustSameWidth);
                cell.Range.Font.Bold = 1;
                cell = cell.Next;

                cell.Range.Text = qdata[i][4];          
                cell = cell.Next;*/

                /*cell.Range.Text = "Description";
                cell.Range.Cells.SetWidth(headerwidth, MSWord.WdRulerStyle.wdAdjustSameWidth);
                cell.Range.Font.Bold = 1;
                cell = cell.Next;*/

                if (!string.IsNullOrWhiteSpace(qdata[i][6]))
                {
                    cell.Merge(cell.Next);
                    cell.Range.Text = qdata[i][6];
                    cell = cell.Next;
                }

                if(!string.IsNullOrWhiteSpace(qdata[i][0]))
                {
                    cell.Range.Text = "Attachments";
                    cell.Range.Cells.SetWidth(headerwidth, MSWord.WdRulerStyle.wdAdjustSameWidth);
                    cell.Range.Font.Bold = 1;
                    cell = cell.Next;

                    var query = qdata[i][0]
                                        .Split(';')
                                        .Where(x => !string.IsNullOrWhiteSpace(x))
                                        .Select(x => 
                                        {
                                            string[] ch = x.Split('~');
                                            return new {name=ch[0], value=ch[1]};
                                        }).ToArray();
                    cell.Split(query.Length, 1);

                    foreach (var kvp in query)
                    {
                        string localpath = Path.GetTempFileName() +"."+ kvp.name;
                        //try { File.Delete(localpath); }
                        //catch { }
                        
                        try
                        {
                            webclient.DownloadFile(kvp.value, localpath);
                        }
                        catch(Exception ex)
                        {
                            localpath = "";
                            Utilities.OutputCommandString(ex.ToString());
                        }

                        prg = document.Paragraphs.Add();
                        prg.Range.Text = kvp.name;
                        prg.Range.set_Style(MSWord.WdBuiltinStyle.wdStyleHeading3);

                        cell.Range.Text = kvp.name;
                        document.Hyperlinks.Add(cell.Range, missing, prg.Range);

                        prg.Range.InsertParagraphAfter();
                        document.InlineShapes.AddHorizontalLineStandard(prg.Range);
                        prg = document.Paragraphs.Add();

                        if (!string.IsNullOrEmpty(localpath))
                        {
                            try
                            {
                                Image img = Image.FromFile(localpath);
                                img.Dispose();
                                document.InlineShapes.AddPicture(localpath, false, true, prg.Range);
                            }
                            catch
                            {
                                if (Path.GetExtension(kvp.name).Equals(".sql", StringComparison.InvariantCultureIgnoreCase) ||
                                    Path.GetExtension(kvp.name).Equals(".txt", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    prg.Range.InsertFile(localpath);//, prg.Range, false, true, false);
                                }
                                else
                                {
                                    MSWord.InlineShape shape = document.InlineShapes.AddOLEObject(missing, localpath, false, false, missing, missing, missing, prg.Range);
                                    if (shape.OLEFormat.ClassType.ToString() != "Package")
                                        shape.Width = document.PageSetup.PageWidth - 40;
                                }
                            }
                        }
                        cell = cell.Next;
                    }
                    if (query.Length == 0) cell = cell.Next;
                }
            }

            object styleTypePara = MSWord.WdStyleType.wdStyleTypeParagraph;
            MSWord.Style styl2 = document.Styles.Add("New Paragraph Style", ref styleTypePara);
            //styl2.ParagraphFormat.set_Style(MSWord.WdBuiltinStyle.wdStyleNormal);
            styl2.ParagraphFormat.LeftIndent = 100;
            styl2.ParagraphFormat.RightIndent = 100;
            styl2.ParagraphFormat.LineSpacingRule = MSWord.WdLineSpacing.wdLineSpaceSingle;
            styl2.ParagraphFormat.SpaceAfter = 0;
            styl2.ParagraphFormat.SpaceBefore = 0;

            MSWord.Paragraph tocpara = document.Paragraphs[3];
            MSWord.TableOfContents tblct = document.TablesOfContents.Add(tocpara.Range, missing, 1, 1);    
            tblct.Update();
            //tblct.Range.set_Style(styl2);  

            //application.Visible = true;

            Marshal.ReleaseComObject(document);
            Marshal.ReleaseComObject(application);

            return false;
        }

        public static MSWord.Style CreateTableStyle(ref MSWord.Document wdDoc)
        {
            MSWord.WdBorderType verticalBorder = MSWord.WdBorderType.wdBorderVertical;
            MSWord.WdBorderType leftBorder = MSWord.WdBorderType.wdBorderLeft;
            MSWord.WdBorderType rightBorder = MSWord.WdBorderType.wdBorderRight;
            MSWord.WdBorderType topBorder = MSWord.WdBorderType.wdBorderTop;
            
            MSWord.WdLineStyle doubleBorder = MSWord.WdLineStyle.wdLineStyleDouble;
            MSWord.WdLineStyle singleBorder = MSWord.WdLineStyle.wdLineStyleSingle;

            MSWord.WdTextureIndex noTexture = MSWord.WdTextureIndex.wdTextureNone;
            MSWord.WdColor gray10 = MSWord.WdColor.wdColorGray10;
            MSWord.WdColor gray70 = MSWord.WdColor.wdColorGray70;
            MSWord.WdColorIndex white = MSWord.WdColorIndex.wdWhite;

            object styleTypeTable = MSWord.WdStyleType.wdStyleTypeTable;
            MSWord.Style styl = wdDoc.Styles.Add("New Table Style", ref styleTypeTable);

            styl.Font.Name = "Arial";
            styl.Font.Size = 11;
            styl.Table.Borders.Enable = 1;

            MSWord.ConditionalStyle evenRowBanding = styl.Table.Condition(MSWord.WdConditionCode.wdEvenRowBanding);
            evenRowBanding.Shading.Texture = noTexture;
            evenRowBanding.Shading.BackgroundPatternColor = gray10;
            // Borders have to be set specifically for every condition.
            evenRowBanding.Borders[leftBorder].LineStyle = doubleBorder;
            evenRowBanding.Borders[rightBorder].LineStyle = doubleBorder;
            evenRowBanding.Borders[verticalBorder].LineStyle = singleBorder;

            MSWord.ConditionalStyle firstRow = styl.Table.Condition(MSWord.WdConditionCode.wdFirstRow);
            firstRow.Shading.BackgroundPatternColor = gray70;
            firstRow.Borders[leftBorder].LineStyle = doubleBorder;
            firstRow.Borders[topBorder].LineStyle = doubleBorder;
            firstRow.Borders[rightBorder].LineStyle = doubleBorder;
            firstRow.Font.Size = 14;
            firstRow.Font.ColorIndex = white;
            firstRow.Font.Bold = 1;

            // Set the number of rows to include in a "band".
            styl.Table.RowStripe = 1;
            return styl;
        }

        public static bool ExecuteQueryLimitedFields(IVsThreadedWaitDialog2 dlg, QueryDefinition qdef, string ProjectName, out string[][] qdata)
        {
            bool bcanceled;
            int progress = 1;
            int locidx = 1;
            int maxlevel = 32;
            WorkItemLinkInfo[] links = null;
            Hashtable context = new Hashtable();
            List<Tag> qresults = new List<Tag>();
            StringBuilder strb = new StringBuilder();
            List<string[]> lqdata = new List<string[]>();

            context.Add("project", ProjectName); //@me, @today are filled automatically
            var query = new Query(Utilities.wistore, qdef.QueryText, context);

            if (query.IsLinkQuery)
            {
                links = query.RunLinkQuery();

                WorkItemLinkTypeEnd[] linkTypes = null;
                if (query.IsTreeQuery)
                {
                    linkTypes = query.GetLinkTypes();
                }
                else
                {
                    maxlevel = 1;
                }

                int idx, flag, level = 0, origidx = 1;
                string origprefix = "";
                foreach (var wilnk in links)
                {
                    if (wilnk.SourceId == 0) 
                        qresults.Add(new Tag() { tWorkItemID = wilnk.TargetId, tlevel = 0, tParentID = 0, tPrefix = (locidx++).ToString() });

                    Utilities.OutputCommandString(string.Format("SourceId={0} TargetId={1} LinkTypeId={2}", wilnk.SourceId, wilnk.TargetId, wilnk.LinkTypeId));
                }

                List<int> chldarr = new List<int>();
                if (qresults.Count > 0)
                {
                    while (level < maxlevel)
                    {
                        level++;
                        flag = 0;
                        locidx = 1;
                        foreach (var wilnk in links.Where(x => x.SourceId > 0))
                        {
                            string lnkname = query.IsTreeQuery ? linkTypes.First(x => x.Id == wilnk.LinkTypeId).ImmutableName : wilnk.LinkTypeId.ToString();

                            idx = -1;
                            if ((idx = qresults.IndexOf(new Tag() { tWorkItemID = wilnk.SourceId, tlevel = level - 1 })) >= 0)
                            {
                                if (origprefix != qresults[idx].tPrefix)
                                {
                                    if (locidx > 2)
                                    {
                                        var qrescopy = qresults.Skip(origidx + 1).Take(locidx - 1);

                                        Utilities.OutputCommandString("level=" + level + ", qrescopy=" + qrescopy.Select(x => x.tPrefix).Aggregate((x, y) => x + "," + y));

                                        var orig = qrescopy.GetEnumerator();
                                        var prefix = qrescopy.Select(x => x.tPrefix).Reverse().GetEnumerator();
                                        while (orig.MoveNext() && prefix.MoveNext()) orig.Current.tPrefix = prefix.Current;
                                    }

                                    locidx = 1;
                                    origprefix = qresults[idx].tPrefix;
                                    origidx = idx;
                                }
                                qresults.Insert(idx + 1, new Tag() { tWorkItemID = wilnk.TargetId, tlevel = level, tLinkTypeID = lnkname, tParentID = wilnk.SourceId, tPrefix = origprefix+"."+(locidx++).ToString() });
                                flag = 1;
                                dlg.UpdateProgress("Exporting Work Item query to Microsoft Word document", "Parsing Work Item #" + wilnk.TargetId.ToString(), "status", progress++, 100, false, out bcanceled);
                                if (progress == 100) progress = 0;
                                if (bcanceled)
                                {
                                    flag = 0;
                                    break;
                                }
                            }
                        }
                        if (flag == 0) break;
                    }
                }

                foreach (var tag in qresults)
                {
                    strb.Clear();
                    WorkItem wi = Utilities.wistore.GetWorkItem(tag.tWorkItemID);

                    strb.Clear();
                    string stratt;
                    foreach (Attachment att in wi.Attachments)
                    {
                        strb.Append(att.Name + "~" + att.Uri.ToString() + ";");
                    }
                    stratt = strb.ToString();
                    Utilities.OutputCommandString(stratt);

                    lqdata.Add(new[] { stratt, 
                                        tag.tlevel.ToString(), 
                                        tag.tPrefix,
                                        wi[CoreFieldReferenceNames.WorkItemType].ToString(),
                                        wi[CoreFieldReferenceNames.Id].ToString(),
                                        wi[CoreFieldReferenceNames.Title].ToString(),
                                        wi[CoreFieldReferenceNames.Description].ToString()});

                    dlg.UpdateProgress("Exporting Work Item query to Microsoft Word document", "Adding to collection Work Item #" + wi.Id.ToString(), "status", progress++, 100, false, out bcanceled);
                    if (progress == 100) progress = 0;
                    if (bcanceled)
                    {
                        break;
                    }
                    Utilities.OutputCommandString(string.Format("ParentID={0}, WorkItemID={1}, Level={2}", tag.tParentID, tag.tWorkItemID, tag.tlevel));
                }
            }
            else
            {
                locidx = 1;
                foreach (WorkItem wi in query.RunQuery())
                {
                    strb.Clear();
                    string stratt;
                    foreach (Attachment att in wi.Attachments)
                    {
                        strb.Append(att.Name + "~" + att.Uri.ToString() + ";");
                    }
                    stratt = strb.ToString();
                    Utilities.OutputCommandString(stratt);

                    string witype = wi[CoreFieldReferenceNames.WorkItemType].ToString();
                    string wiid = wi[CoreFieldReferenceNames.Id].ToString();
                    lqdata.Add(new[] { stratt, "0",
                                        (locidx++).ToString(),
                                        witype,
                                        wiid,
                                        wi[CoreFieldReferenceNames.Title].ToString(),
                                        wi[CoreFieldReferenceNames.Description].ToString()});

                    dlg.UpdateProgress("Exporting Work Item query to Microsoft Word document", "Parsing " + witype + " #" + wiid, "status", progress++, 100, false, out bcanceled);
                    if (progress == 100) progress = 0;
                    if(bcanceled)
                    {
                        break;
                    }
                }
            }

            qdata = lqdata.ToArray();

            return false;
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        public static void MenuItemCallback(object sender, EventArgs e)
        {
            object _lockToken1 = new object();
            object _lockToken2 = new object();

            IWorkItemTrackingDocument doc = Utilities.docsrv2.FindDocument(Utilities.dte.ActiveDocument.FullName, _lockToken2);
            if (doc == null) return;

            int fldid;
            bool isSortable;
            var dataprov = (doc as IResultsDocument).ResultListDataProvider;
            dataprov.GetFieldInfo("field name", out fldid, out isSortable);

            var qdoc = (doc as IResultsDocument).QueryDocument;// as QueryDocument;      
            /*
Microsoft.VisualStudio.TeamFoundation.WorkItemTracking.PackageHelper.OpenQueryInOffice()
Microsoft.VisualStudio.TeamFoundation.WorkItemTracking.QueryDocument

Microsoft.TeamFoundation.OfficeIntegration.Client.DocumentLaunch
Microsoft.TeamFoundation.OfficeIntegration.Client.ExcelAddIn.CreateDocumentInternal()
Microsoft.TeamFoundation.OfficeIntegration.Client.ELeadWorkbook.CreateList()
             * /

            foreach (int i in (doc as IResultsDocument).SelectedItemIds)
            {
                string val = dataprov.GetFieldValue(dataprov.GetItemIndex(i), fldid);
                /*IWorkItemDocument widoc = docsrv.GetWorkItem(tfscoll, i, _lockToken1);
                if (widoc == null) continue;


                widoc.Load();

                wicoll.Add(widoc.Item.Id);

                widoc.Release(_lockToken1);
            }*/
            doc.Release(_lockToken2);

            //wistore.DestroyWorkItems(wicoll);
        }
    }
}
