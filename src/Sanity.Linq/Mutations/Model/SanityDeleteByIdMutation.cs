﻿// Copywrite 2018 Oslofjord Operations AS

// This file is part of Sanity LINQ (https://github.com/oslofjord/sanity-linq).

//  Sanity LINQ is free software: you can redistribute it and/or modify
//  it under the terms of the MIT Licence.

//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//  MIT Licence for more details.

//  You should have received a copy of the MIT Licence
//  along with this program.

using System;
using System.Collections.Generic;
using System.Text;

namespace Sanity.Linq.Mutations
{
    public class SanityDeleteByIdMutation : SanityMutation
    {
        public SanityDeleteByIdMutation(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Id cannot be null when creating a delete mutation.", nameof(id));
            }

            Delete = new SanityId { Id = id };
        }

        public SanityId Delete { get; set; }

        public class SanityId
        {
            public string Id { get; set; }
        }
    }
}
