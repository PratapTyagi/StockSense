import React from 'react';
import './ComparisonTable.css';

type ComparisonRow<T> = {
  label: string;
  accessor?: keyof T;
  render?: (item: T) => React.ReactNode;
};

type ComparisonTableProps<T> = {
  items: T[]; // columns (things being compared)
  rows: ComparisonRow<T>[]; // rows (attributes)
  getItemKey?: (item: T, index: number) => string | number;
  getItemLabel?: (item: T, index: number) => React.ReactNode;
};

function ComparisonTable<T extends Record<string, any>>({
  items,
  rows,
  getItemKey,
  getItemLabel,
}: ComparisonTableProps<T>) {
  return (
    <div className="comparison-table-container relative overflow-x-auto shadow-sm rounded-base border-default">
      <table className="w-full text-sm text-left text-body">
        <thead>
          <tr>
            <th className="font-medium">Metric</th>
            {items.map((item, index) => (
              <th key={getItemKey?.(item, index) ?? index}>
                {getItemLabel?.(item, index) ??
                  item.name ??
                  `Item ${index + 1}`}
              </th>
            ))}
          </tr>
        </thead>

        <tbody>
          {rows.map((row, rowIndex) => (
            <tr key={rowIndex}>
              <td className="font-medium nowrap">{row.label}</td>

              {items.map((item, colIndex) => {
                const value = row.accessor
                  ? item[row.accessor]
                  : undefined;

                return (
                  <td key={colIndex}>
                    {row.render
                      ? row.render(item)
                      : value}
                  </td>
                );
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default ComparisonTable;
