import { useEffect, useState } from 'react';
import { msmsApi } from '../../api/msmsApi';
import type { LinkedStudent } from '../../types';
import { Card, CardBody, CardHeader, EmptyState, PageHeader, TableWrap, Td, Th } from '../../components/ui';

export function ChildrenPage() {
  const [children, setChildren] = useState<LinkedStudent[]>([]);

  useEffect(() => {
    msmsApi.parent.children().then((r) => setChildren(r.data));
  }, []);

  return (
    <>
      <PageHeader title="My Children" description="Students linked to your account" />
      <Card>
        <CardHeader title="Children" subtitle={`${children.length} linked`} />
        <CardBody className="p-0">
          {children.length === 0 ? (
            <EmptyState message="No children linked. Contact the school admin." />
          ) : (
            <TableWrap>
              <thead><tr><Th>ID</Th><Th>Name</Th><Th>Class</Th></tr></thead>
              <tbody>
                {children.map((c) => (
                  <tr key={c.id}>
                    <Td>{c.studentNumber}</Td>
                    <Td className="font-medium">{c.firstName} {c.lastName}</Td>
                    <Td>{c.classId ? 'Assigned' : '—'}</Td>
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
