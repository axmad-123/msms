import { useEffect, useState } from 'react';
import { msmsApi } from '../../api/msmsApi';
import type { TeacherAssignment } from '../../types';
import { Card, CardBody, CardHeader, EmptyState, PageHeader, TableWrap, Td, Th } from '../../components/ui';

export function TeacherAssignmentsPage() {
  const [items, setItems] = useState<TeacherAssignment[]>([]);

  useEffect(() => {
    msmsApi.teacher.assignments().then((r) => setItems(r.data));
  }, []);

  return (
    <>
      <PageHeader title="My Classes" description="Subjects and classes assigned to you" />
      <Card>
        <CardHeader title="Assignments" subtitle={`${items.length} active`} />
        <CardBody className="p-0">
          {items.length === 0 ? (
            <EmptyState message="No assignments yet. Contact admin." />
          ) : (
            <TableWrap>
              <thead><tr><Th>Class</Th><Th>Subject</Th></tr></thead>
              <tbody>
                {items.map((a, i) => (
                  <tr key={`${a.classId}-${a.subjectId}-${i}`}>
                    <Td className="font-medium">{a.className}</Td>
                    <Td>{a.subjectName}</Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          )}
        </CardBody>
      </Card>
    </>
  );
}
