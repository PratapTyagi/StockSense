import React from 'react';
import './Table.css';

export type Column<T> = {
  header: string;
  accessor?: keyof T;
  render?: (row: T, value?: any) => React.ReactNode;
  className?: string;
  align?: 'left' | 'right' | 'center';
};

type TableProps<T> = {
  columns: Column<T>[];
  data: T[];
  rowKey?: keyof T;
  onRowClick?: (row: T) => void;
};

function Table<T extends Record<string, any>>({
  columns,
  data,
  rowKey,
  onRowClick,
}: TableProps<T>) {
  return (
    <div className="table-container relative overflow-x-auto shadow-sm rounded-base border-default">
      <table className="w-full text-sm text-left text-body">
        <thead>
          <tr>
            {columns.map((col, index) => (
              <th key={index}>{col.header}</th>
            ))}
          </tr>
        </thead>

        <tbody>
          {data.map((row, rowIndex) => (
            <tr
              key={(rowKey && row[rowKey]) ?? rowIndex}
              onClick={() => onRowClick?.(row)}
            >
              {columns.map((col, colIndex) => {
                const value = col.accessor ? row[col.accessor] : undefined;

                return (
                  <td
                    key={colIndex}
                    className={`
                      ${col.className || ''}
                      ${col.align === 'right' ? 'text-right' : ''}
                      ${col.align === 'center' ? 'text-center' : ''}
                    `}
                  >
                    {col.render ? col.render(row, value) : value}
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

export default Table;
