﻿using System.Collections.Generic;
using MiniJSON;
using UnityEngine;

namespace EA4S.Db.Management
{
    public class PhraseParser : DataParser<PhraseData, PhraseTable>
    {
        override protected PhraseData CreateData(Dictionary<string, object> dict, Database db)
        {
            var data = new PhraseData();

            data.Id = ToString(dict["Id"]);
            data.English = ToString(dict["English"]);
            data.Arabic = ToString(dict["Arabic"]);
            data.Linked = ToString(dict["Linked"]);

            return data;
        }

        protected override void RegenerateEnums(List<Dictionary<string, object>> rowdicts_list)
        {
        }

        protected override void FinalValidation(PhraseTable table)
        {
            // Field 'Linked' is validated with a final validation step, since it is based on this same table
            foreach(var data in table.GetValuesTyped())
            {
                if (data.Linked != "" && table.GetValue(data.Linked) == null)
                {
                    LogValidation(data, "Cannot find id of PhraseData for Linked value " + data.Linked + " (found in phrase " + data.Id + ")");
                }
                else
                    Debug.Log("CORRECT LINKED " + data.Linked);
            }

        }

    }
}
