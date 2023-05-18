/* Copyright (c) Mark Seemann 2020. All rights reserved. */
﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Ploeh.Samples.Restaurants.RestApi
{
    public sealed class Table
    {
        private readonly bool isStandard;
        private readonly Reservation[] reservations;

        private Table(
            bool isStandard,
            int seats,
            params Reservation[] reservations)
        {
            this.isStandard = isStandard;
            Capacity = seats;
            this.reservations = reservations;
        }

        public static Table Standard(int seats)
        {
            return new Table(true, seats);
        }

        public static Table Communal(int seats)
        {
            return new Table(false, seats);
        }

        public int Capacity { get; }

        public int RemainingSeats
        {
            get
            {
                if (isStandard)
                    return reservations.Any() ? 0 : Capacity;
                else
                    return Capacity - reservations.Sum(r => r.Quantity);
            }
        }

        internal bool Fits(int quantity)
        {
            return quantity <= RemainingSeats;
        }

        public Table Reserve(Reservation reservation)
        {
            if (isStandard)
                return new Table(isStandard, Capacity, reservation);
            else
                return new Table(
                    isStandard,
                    Capacity,
                    reservations.Append(reservation).ToArray());
        }

        public T Accept<T>(ITableVisitor<T> visitor)
        {
            if (visitor is null)
                throw new ArgumentNullException(nameof(visitor));

            if (isStandard)
                return visitor.VisitStandard(
                    Capacity,
                    reservations.Any() ? reservations.First() : null);
            else
                return visitor.VisitCommunal(Capacity, reservations.ToList());
        }

        public override bool Equals(object? obj)
        {
            return obj is Table table &&
                   isStandard == table.isStandard &&
                   Capacity == table.Capacity &&
                   reservations.SequenceEqual(table.reservations);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(isStandard, Capacity, reservations);
        }
    }
}
