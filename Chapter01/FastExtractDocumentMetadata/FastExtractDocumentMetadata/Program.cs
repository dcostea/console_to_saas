﻿using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace FastExtractDocumentMetadata
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("please enter the folder with financial Word documents( press enter for current folder)");
            string folderWithWordDocs = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(folderWithWordDocs))
            {
                folderWithWordDocs = ".";
                
            }

            string[] files = Directory.GetFiles(folderWithWordDocs, "*.docx");
            Console.WriteLine($"processing {files?.Length} word documents");
            List<object[]> allContractors = new List<object[]>();
            foreach (string file in files)
            {
                Console.WriteLine($"start processing {file}");
                XWPFDocument document = new XWPFDocument(File.OpenRead(file));

                if (document.Tables.Count < 2)
                {
                    throw new InvalidOperationException("Expected at least 2 tables");
                }

                var contractorDetails = ExactContractorDetails(document.Tables[0]);
                allContractors.Add(contractorDetails);

                Console.WriteLine($"end processing {file}");

            }
            {
                var wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet("Contractors");
                for (int i = 0; i < allContractors.Count; i++)
                {
                    var contractor = allContractors[i];
                    IRow row = sheet.CreateRow(i);
                    for (int j = 0; j < contractor.Length; j++)
                    {
                        var cell = row.CreateCell(j);
                        cell.SetCellValue(contractor[j].ToString());
                    }
                }
                wb.Write(File.OpenWrite("Contractors.xlsx"));
                Console.WriteLine("please see Contractors.xlsx");
            }
        }
        private static object[] ExactContractorDetails(XWPFTable table)
        {
            var rowItems = new List<object>();

            foreach (XWPFTableRow row in table.Rows)
            {
                List<XWPFTableCell> cells = row.GetTableCells();
                for (int i = 1; i < cells.Count; i += 2)
                {
                    rowItems.Add(cells[i].GetText());
                }
            }

            return rowItems.ToArray();
        }
    }
}

