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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sanity.Linq.Extensions;
using Sanity.Linq.Internal;

namespace Sanity.Linq
{
    public class SanityQueryProvider : IQueryProvider
    {
        private bool _excludeDocTypeConstraint;
        private object _queryBuilderLock = new object();
        public Type DocType { get; }
        public SanityDataContext Context { get; }

        public int MaxNestingLevel { get; }

        public SanityQueryProvider(Type docType, SanityDataContext context, int maxNestingLevel, bool excludeDocTypeConstraint = false)
        {
            _excludeDocTypeConstraint = excludeDocTypeConstraint;
            MaxNestingLevel = maxNestingLevel;
            DocType = docType;
            Context = context;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(SanityDocumentSet<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        // Queryable's collection-returning standard query operators call this method. 
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new SanityDocumentSet<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            return Execute<object>(expression);
        }

        // Queryable's "single value" standard query operators call this method.
        public TResult Execute<TResult>(Expression expression)
        {
            return ExecuteAsync<TResult>(expression).Result;            
        }

        public string GetSanityQuery<TResult>(Expression expression)
        {
            var parser = new SanityExpressionParser(expression, DocType, MaxNestingLevel, typeof(TResult), _excludeDocTypeConstraint);
            return parser.BuildQuery();
        }

        public string GetSanityQuery(Expression expression, Type resultType)
        {
            var parser = new SanityExpressionParser(expression, DocType, MaxNestingLevel, resultType, _excludeDocTypeConstraint);
            return parser.BuildQuery();
        }

        public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var query = GetSanityQuery(expression, typeof(TResult));          

            // Execute query
            var result = await Context.Client.FetchAsync<TResult>(query, null, cancellationToken).ConfigureAwait(false);

            return result.Result;

        }

        public void ExcludeDocTypeConstraint()
        {
            _excludeDocTypeConstraint = true;
        }
    }


}
