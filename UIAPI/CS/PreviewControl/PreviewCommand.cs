//
// (C) Copyright 2003-2016 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RApplication = Autodesk.Revit.ApplicationServices.Application;
using Exceptions = Autodesk.Revit.Exceptions;
using Autodesk.Revit.ApplicationServices;

namespace Revit.SDK.Samples.UIAPI.CS
{
   /// <summary>
   /// Implement this method as an external command for Revit.
   /// </summary>
   /// <param name="commandData">An object that is passed to the external application
   /// which contains data related to the command,
   /// such as the application object and active view.</param>
   /// <param name="message">A message that can be set by the external application
   /// which will be displayed if a failure or cancellation is returned by
   /// the external command.</param>
   /// <param name="elements">A set of elements to which the external application
   /// can add elements that are to be highlighted in case of failure or cancellation.</param>
   /// <returns>Return the status of the external command.
   /// A result of Succeeded means that the API external method functioned as expected.
   /// Cancelled can be used to signify that the user cancelled the external operation 
   /// at some point. Failure should be returned if the application is unable to proceed with
   /// the operation.</returns>
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   public class PreviewCommand : IExternalCommand
   {
      public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
      {
         _dbdocument = commandData.Application.ActiveUIDocument.Document;

   
         TransactionGroup outerGroup = new TransactionGroup(_dbdocument, "preview control");
         outerGroup.Start();

         try
         {
                createAndOrient(commandData);

                //PreviewModel form = new PreviewModel(commandData.Application.Application, new ElementId(-1));
                //form.ShowDialog();
                
         }
         catch (Exception e)
         {
            throw e;
         }
         finally
         {
            outerGroup.RollBack();
         }

         return Result.Succeeded;
      }



      public static void SelectDelete(ExternalCommandData commandData)
      {
            /// <summary>
            /// store the application
            /// </summary>
            UIApplication m_application;
            /// <summary>
            /// store the document
            /// </summary>
            UIDocument m_document;

            m_application = commandData.Application;
            m_document = m_application.ActiveUIDocument;

            Transaction trans = new Transaction(m_document.Document, "PickforDeletion");
            trans.Start();

           

            try
            {
                // Select elements. Click "Finish" or "Cancel" buttons on the dialog bar to complete the selection operation.
                List<ElementId> elemDeleteList = new List<ElementId>();
                IList<Reference> eRefList = m_document.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, "Please pick some element to delete. ESC for Cancel.");


                foreach (Reference eRef in eRefList)
                {

                    if (eRef != null && eRef.ElementId != ElementId.InvalidElementId)
                    {
                        elemDeleteList.Add(eRef.ElementId);


                    }
                }

                // Delete elements
                m_document.Document.Delete(elemDeleteList);
                trans.Commit();
                //return Result.Succeeded;
            }
            catch (Exceptions.OperationCanceledException)
            {
                // Selection Cancelled.
                trans.RollBack();
                return;
                //return Result.Cancelled;
            }
            catch (Exception ex)
            {
                // If any error, give error information and return failed
                //message = ex.Message;
                trans.RollBack();
                return;
                //return Result.Failed;
            }

        }

        

      public void createAndOrient(ExternalCommandData commandData)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            RApplication app = doc.Application;
            FamilySymbol famSym = null;

            /*
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Load Family");
                doc.LoadFamily("C:/Users/jane0/Desktop/IES_revit_plugin/Beacon NEW.rfa", out family);
                tx.Commit();
            }
            */


            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Load Family");
                /*
                // Allow the user to select a family file.
                System.Windows.Forms.OpenFileDialog openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
                openFileDialog1.InitialDirectory = "C:/Users/jane0/Desktop/IES_revit_plugin";
                //openFileDialog1.InitialDirectory = "C:/Autodesk/WI/Autodesk Revit 2017/x64/RVT/Program Files/Autodesk/Root/Samples/";
                openFileDialog1.Filter = "Family Files (*.rfa)|*.rfa"; 
                

                // Load the family file using LoadFamily method and then give information.
                
                if (System.Windows.Forms.DialogResult.OK == openFileDialog1.ShowDialog())
                {
                    //if (doc.LoadFamilySymbol(openFileDialog1.FileName, "0610 x 0160mm", out famSym))
                    if (doc.LoadFamilySymbol(openFileDialog1.FileName, "High Ceiling", out famSym))
                    {
                        TaskDialog.Show("Revit", "load the family file, success");
                    }
                    else
                    {
                        TaskDialog.Show("Revit", "Can't load the family file."+ openFileDialog1.FileName);
                    }
                }
               
                */
                //if (!doc.LoadFamilySymbol("C:/Users/jane0/Desktop/IES_revit_plugin/Beacon NEW.rfa", "High Ceiling", out famSym))
                if (!doc.LoadFamilySymbol("C:/Autodesk/WI/Autodesk Revit 2017/x64/RVT/Program Files/Autodesk/Root/Samples/basic_sample_family.rfa", "0610 x 0160mm", out famSym))
                {
                        TaskDialog.Show("Revit", "Can't load the family file.");
                }
                
                tx.Commit();
            }

            // use a transaction group so that all the individual transactions are merged into a single entry in the Undo menu
            // this is optional
            string str = "";
            
            // create an infinite loop so user can create multiple instances in a single command
            // ESC when prompted to select a point will thrown an exception which is how the loop is exited
            while (true)
            {
                    try
                    {
                        XYZ pickPoint = uidoc.Selection.PickPoint("Click to specify instance location. ESC to stop placing instances.");
                        
                        FamilyInstance familyInstance = null;
                        // Create the instance with the default orientation
                        // This is done in its own transaction so that the user can see the new instance when they are prompted for the orientation
                        try
                        {
                            using (Transaction t = new Transaction(doc, "Place Instance"))
                            {
                                t.Start();

                                str += pickPoint + " " + famSym.Name + " " + doc.ActiveView.Name + "\n";

                                familyInstance = doc.Create.NewFamilyInstance(pickPoint, famSym, doc.ActiveView);
                            
                                t.Commit();
                            }
                        }
                        catch
                        {
                            TaskDialog.Show("Revit", str + "NewFamilyInstance failed");
                            break;
                        }


                    }
                    catch
                    {
                        TaskDialog.Show("Revit", str + "After create new instance");
                        // Get here when the user hits ESC when prompted for selection
                        // "break" exits from the while loop
                        break;
                    }
             }
               
        }



        private Document _dbdocument = null;
   }
   
}
